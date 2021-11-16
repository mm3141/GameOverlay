namespace SimpleFlaskManager.ProfileManager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;
    using Conditions;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;
    using SimpleFlaskManager.ProfileManager.Enums;

    /// <summary>
    /// Abstraction for the rule condition list
    /// </summary>
    public class Rule
    {
        private ConditionType newConditionType;
        private readonly Stopwatch delayStopwatch = Stopwatch.StartNew();

        [JsonProperty("Conditions", NullValueHandling = NullValueHandling.Ignore)]
        private readonly List<ICondition> conditions = new();

        [JsonProperty] private float delayBetweenRuns = 0;

        /// <summary>
        ///     Enable/Disable the rule.
        /// </summary>
        public bool Enabled;

        /// <summary>
        ///     User friendly name given to a rule.
        /// </summary>
        public string Name;

        /// <summary>
        ///     Rule key to press on success.
        /// </summary>
        public ConsoleKey Key;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Rule" /> class.
        /// </summary>
        /// <param name="name"></param>
        public Rule(string name)
        {
            this.Name = name;
        }

        /// <summary>
        ///     Clears the list of conditions
        /// </summary>
        public void Clear()
        {
            this.conditions.Clear();
        }

        /// <summary>
        ///     Displays the rule settings
        /// </summary>
        public void DrawSettings()
        {
            ImGui.Checkbox("Enable", ref this.Enabled);
            ImGui.InputText("Name", ref this.Name, 20);
            ImGuiHelper.NonContinuousEnumComboBox("Key", ref this.Key);
            this.DrawCooldownWidget();
            this.DrawAddNewCondition();
            this.DrawExistingConditions();
        }

        /// <summary>
        ///     Checks the rule conditions and presses its key if conditions are satisfied
        /// </summary>
        /// <param name="logger"></param>
        public void Execute(Action<string> logger)
        {
            if (this.Enabled && this.Evaluate())
            {
                if (MiscHelper.KeyUp(this.Key))
                {
                    logger($"Pressed the {this.Key} key");
                }
            }
        }

        /// <summary>
        ///     Adds a new condition
        /// </summary>
        /// <param name="conditionType"></param>
        private void Add(ConditionType conditionType)
        {
            var condition = EnumToObject(conditionType);
            if (condition != null)
            {
                this.conditions.Add(condition);
            }
        }

        /// <summary>
        ///     Removes a condition at a specific index.
        /// </summary>
        /// <param name="index">index of the condition to remove.</param>
        private void RemoveAt(int index)
        {
            this.conditions.RemoveAt(index);
        }

        /// <summary>
        ///     Swap two conditions.
        /// </summary>
        /// <param name="i">index of the condition to swap.</param>
        /// <param name="j">index of the condition to swap.</param>
        private void Swap(int i, int j)
        {
            (this.conditions[i], this.conditions[j]) = (this.conditions[j], this.conditions[i]);
        }

        /// <summary>
        ///     Checks the specified conditions, shortcircuiting on the first unsatisfied one
        /// </summary>
        /// <returns>true if all the rules conditions are true otherwise false.</returns>
        private bool Evaluate()
        {
            if (this.delayStopwatch.Elapsed.TotalSeconds > this.delayBetweenRuns)
            {
                if (this.conditions.TrueForAll(x => x.Evaluate()))
                {
                    this.delayStopwatch.Restart();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Converts the <see cref="ConditionType" /> to the appropriate <see cref="ICondition" /> object.
        /// </summary>
        /// <param name="conditionType">Condition type to create.</param>
        /// <returns>
        ///     Returns <see cref="ICondition" /> if user wants to create it or null.
        ///     Throws an exception in case it doesn't know how to create a specific Condition.
        /// </returns>
        private static ICondition EnumToObject(ConditionType conditionType)
        {
            ICondition p = conditionType switch
            {
                ConditionType.VITALS => VitalsCondition.Add(),
                ConditionType.ANIMATION => AnimationCondition.Add(),
                ConditionType.STATUS_EFFECT => StatusEffectCondition.Add(),
                ConditionType.FLASK_EFFECT => FlaskEffectCondition.Add(),
                ConditionType.FLASK_CHARGES => FlaskChargesCondition.Add(),
                ConditionType.AILMENT => AilmentCondition.Add(),
                _ => throw new Exception($"{conditionType} not implemented in ConditionHelper class")
            };

            return p;
        }

        private void DrawCooldownWidget()
        {
            ImGui.DragFloat("Cooldown time (seconds)##DelayTimerConditionDelay", ref this.delayBetweenRuns, 0.1f, 0.0f, 30.0f);
            ImGui.SameLine();
            var cooldownTimeFraction = this.delayBetweenRuns <= 0f ? 1f :
                MathF.Min((float)this.delayStopwatch.Elapsed.TotalSeconds, this.delayBetweenRuns) / this.delayBetweenRuns;
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, cooldownTimeFraction < 1f ?
                ImGuiHelper.Color(255, 0, 0, 255) : ImGuiHelper.Color(0, 255, 0, 255));
            ImGui.ProgressBar(
                (float)cooldownTimeFraction,
                new Vector2(ImGui.GetContentRegionAvail().X, 0f),
                cooldownTimeFraction < 1f ? "Cooling" : "Ready");
            ImGui.PopStyleColor();
        }

        private void DrawExistingConditions()
        {
            if (ImGui.TreeNode("Existing Conditions (all of them have to be true)"))
            {
                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X / 6);
                for (var i = 0; i < this.conditions.Count; i++)
                {
                    ImGui.PushID($"ConditionNo{i}");
                    if (i != 0)
                    {
                        ImGui.Separator();
                    }

                    if (ImGui.Button("Delete"))
                    {
                        this.RemoveAt(i);
                        ImGui.PopID();
                        break;
                    }

                    ImGui.SameLine();
                    if (ImGui.ArrowButton("up", ImGuiDir.Up) && i > 0)
                    {
                        ImGui.PopStyleVar();
                        this.Swap(i, i - 1);
                        ImGui.PopID();
                        break;
                    }

                    ImGui.SameLine();
                    if (ImGui.ArrowButton("down", ImGuiDir.Down) && i != this.conditions.Count - 1)
                    {
                        this.Swap(i, i + 1);
                        ImGui.PopID();
                        break;
                    }

                    ImGui.SameLine();
                    this.conditions[i].Display();
                    ImGui.PopID();
                }

                ImGui.PopItemWidth();
                ImGui.TreePop();
            }
        }

        private void DrawAddNewCondition()
        {
            if (ImGui.Button("Add New Condition"))
            {
                ImGui.OpenPopup("AddNewConditionPopUp");
            }

            if (ImGui.BeginPopup("AddNewConditionPopUp"))
            {
                ImGuiHelper.EnumComboBox("Condition", ref this.newConditionType);
                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X / 6);
                ImGui.Separator();
                this.Add(this.newConditionType);
                ImGui.PopItemWidth();
                if (ImGui.Button("Done", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetTextLineHeight() * 2)))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }
    }
}
