// <copyright file="GameUiExplorer.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Ui
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using GameHelper.RemoteObjects.UiElement;
    using GameHelper.Utils;
    using ImGuiNET;

    /// <summary>
    /// Explore (visualize) the Game Ui Elements.
    /// </summary>
    public static class GameUiExplorer
    {
        private static List<UiElement> elements = new List<UiElement>();

        /// <summary>
        /// Initializes the co-routines.
        /// </summary>
        internal static void InitializeCoroutines()
        {
            CoroutineHandler.Start(GameUiExplorerRenderCoRoutine());
        }

        /// <summary>
        /// Adds the UiElementBase to GameUiExplorer.
        /// </summary>
        /// <param name="element">UiElementBase to investigate.</param>
        internal static void AddUiElement(UiElementBase element)
        {
            elements.Add(CreateUiElement(element));
            Core.GHSettings.ShowGameUiExplorer = true;
        }

        private static UiElement CreateUiElement(UiElementBase element)
        {
            UiElement eleStruct = new UiElement()
            {
                CurrentChildIndex = -1,
                CurrentChildPreview = string.Empty,
                Element = element,
                Children = new List<UiElementBase>(),
            };

            for (int i = 0; i < element.TotalChildrens; i++)
            {
                eleStruct.Children.Add(element[i]);
            }

            return eleStruct;
        }

        private static void RemoveUiElement(int i)
        {
            elements[i].Children.Clear();
            elements.RemoveAt(i);
        }

        /// <summary>
        /// Draws the window to display the Game UiExplorer.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        private static IEnumerator<Wait> GameUiExplorerRenderCoRoutine()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnRender);
                if (!Core.GHSettings.ShowGameUiExplorer)
                {
                    continue;
                }

                if (ImGui.Begin("Game UiExplorer", ref Core.GHSettings.ShowGameUiExplorer))
                {
                    if (ImGui.TreeNode("NOTES"))
                    {
                        ImGui.BulletText("Closing the game will remove all objects.");
                        ImGui.BulletText("To add element in this window go to any UiElement " +
                            "in\nData Visualization window and click Explore button.");
                        ImGui.BulletText("To check currently loaded element bounds,\n" +
                            "hover over the element header in blue.");
                        ImGui.BulletText("To check bounds of all the children hover\n" +
                            "over the Children box.");
                        ImGui.BulletText("Feel free to add same element more than once.");
                        ImGui.BulletText("When children combo box is opened feel free\n" +
                            "to use the up/down arrow key.");
                        ImGui.BulletText("Children bounds are drawn with RED color.");
                        ImGui.BulletText("Current element bounds are drawn with Yellow Color.");

                        ImGui.TreePop();
                    }

                    if (ImGui.Button("Clear all Ui Elements (Mischief managed)") ||
                        Core.Process.Address == IntPtr.Zero)
                    {
                        for (int i = elements.Count - 1; i >= 0; i--)
                        {
                            RemoveUiElement(i);
                        }
                    }

                    ImGui.Separator();
                    for (int i = 0; i < elements.Count; i++)
                    {
                        var current = elements[i];
                        string eleName = $"{current.Element.Id}";
                        bool isRequired = true;
                        bool isCurrentModified = false;
                        bool isEnterPressed = false;
                        if (ImGui.CollapsingHeader(eleName + $"##{i}", ref isRequired))
                        {
                            if (ImGui.IsItemHovered())
                            {
                                UiHelper.DrawRect(
                                    current.Element.Postion,
                                    current.Element.Size,
                                    255,
                                    255,
                                    0);
                            }

                            UiHelper.IntPtrToImGui(eleName + ": ", current.Element.Address);
                            current.Element.Address = current.Element.Address;
                            if (ImGui.BeginCombo($"Children##{i}", current.CurrentChildPreview))
                            {
                                if (current.CurrentChildIndex > -1)
                                {
                                    if (ImGui.IsItemHovered())
                                    {
                                        var cChild = current.Children[current.CurrentChildIndex];
                                        UiHelper.DrawRect(
                                            cChild.Postion,
                                            cChild.Size,
                                            255,
                                            64,
                                            64);
                                    }
                                }

                                if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.UpArrow)))
                                {
                                    if (current.CurrentChildIndex > 0)
                                    {
                                        current.CurrentChildIndex--;
                                    }
                                    else
                                    {
                                        current.CurrentChildIndex = current.Children.Count - 1;
                                    }

                                    isCurrentModified = true;
                                    elements[i] = current;
                                }
                                else if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.DownArrow)))
                                {
                                    if (current.CurrentChildIndex < current.Children.Count - 1)
                                    {
                                        current.CurrentChildIndex++;
                                    }
                                    else
                                    {
                                        current.CurrentChildIndex = 0;
                                    }

                                    isCurrentModified = true;
                                    elements[i] = current;
                                }
                                else if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.Enter)))
                                {
                                    isEnterPressed = true;
                                }

                                for (int j = 0; j < current.Children.Count; j++)
                                {
                                    bool selected = j == current.CurrentChildIndex;
                                    var child = current.Children[j];
                                    child.Address = child.Address;

                                    if (ImGui.Selectable($"{child.Address}-{child.Id}", selected))
                                    {
                                        current.CurrentChildIndex = j;
                                        current.CurrentChildPreview = $"{child.Address}-{child.Id}";
                                        elements[i] = current;
                                    }

                                    if (isEnterPressed && selected)
                                    {
                                        current.CurrentChildIndex = j;
                                        current.CurrentChildPreview = $"{child.Address}-{child.Id}";
                                        elements[i] = current;
                                        ImGui.CloseCurrentPopup();
                                        isEnterPressed = false;
                                    }

                                    if (ImGui.IsWindowAppearing() || isCurrentModified)
                                    {
                                        if (selected)
                                        {
                                            isCurrentModified = false;
                                            ImGui.SetScrollHereY();
                                        }
                                    }

                                    if (ImGui.IsItemHovered())
                                    {
                                        UiHelper.DrawRect(
                                            child.Postion,
                                            child.Size,
                                            255,
                                            64,
                                            64);
                                    }
                                }

                                ImGui.EndCombo();
                            }

                            if (ImGui.IsItemHovered())
                            {
                                for (int j = 0; j < current.Children.Count; j++)
                                {
                                    var child = current.Children[j];
                                    child.Address = child.Address;
                                    UiHelper.DrawRect(child.Postion, child.Size, 255, 64, 64);
                                }
                            }

                            if (current.Children.Count > 0 && current.CurrentChildIndex > -1)
                            {
                                if (ImGui.Button($"Go to child##{i}"))
                                {
                                    elements[i] = CreateUiElement(current.Children[current.CurrentChildIndex]);
                                }
                            }
                            else
                            {
                                UiHelper.DrawDisabledButton($"Go to child##{i}");
                            }

                            ImGui.SameLine();
                            if (current.Element.Parent != null)
                            {
                                if (ImGui.Button($"Go to parent##{i}"))
                                {
                                    elements[i] = CreateUiElement(current.Element.Parent);
                                }
                            }
                            else
                            {
                                UiHelper.DrawDisabledButton($"Go to parent##{i}");
                            }
                        }

                        if (!isRequired)
                        {
                            RemoveUiElement(i);
                        }
                    }
                }

                ImGui.End();
            }
        }

        private struct UiElement
        {
            public int CurrentChildIndex;
            public string CurrentChildPreview;
            public UiElementBase Element;
            public List<UiElementBase> Children;
        }
    }
}
