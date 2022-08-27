// <copyright file="DynamicCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger.ProfileManager.Conditions.DynamicCondition
{
    using System;
    using System.Linq.Dynamic.Core;
    using System.Numerics;
    using AutoHotKeyTrigger.ProfileManager.Component;
    using GameHelper;
    using ImGuiNET;
    using Newtonsoft.Json;

    /// <summary>
    ///     A customizable condition allowing to specify when it is satisfied in user-supplied code
    /// </summary>
    public class DynamicCondition : ICondition
    {
        private static readonly Vector4 ConditionSuccess = new(0, 255, 0, 255);
        private static readonly Vector4 ConditionFailure = new(255, 255, 0, 255);
        private static readonly Vector4 CodeCompileFailure = new(255, 0, 0, 255);
        private static readonly DynamicCondition ConfigurationInstance = new("");

        private static Lazy<DynamicConditionState> state;

        [JsonProperty] private string conditionSource;
        private string lastException;
        private Func<DynamicConditionState, bool> func;
        private ulong exceptionCounter = 0;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicCondition" /> class.
        /// </summary>
        /// <param name="conditionSource">
        ///     The source code for the condition
        /// </param>
        public DynamicCondition(string conditionSource)
        {
            this.conditionSource = conditionSource;
            this.RebuildFunction();
        }
        
        static DynamicCondition()
        {
            UpdateState();
        }

        /// <summary>
        ///     Indicates that the shared dynamic condition state needs to be rebuilt
        /// </summary>
        public static void UpdateState()
        {
            state = new Lazy<DynamicConditionState>(() => new DynamicConditionState(Core.States.InGameStateObject));
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public static DynamicCondition Add()
        {
            ConfigurationInstance.ToImGui();
            ImGui.SameLine();
            if (ImGui.Button("Add##StatusEffect") &&
                !string.IsNullOrEmpty(ConfigurationInstance.conditionSource))
            {
                return new DynamicCondition(ConfigurationInstance.conditionSource);
            }

            return null;
        }

        /// <inheritdoc />
        public void Display(bool expand)
        {
            this.ToImGui(expand);
        }

        /// <inheritdoc/>
        public void Add(IComponent component)
        {
        }

        /// <inheritdoc />
        public bool Evaluate()
        {
            return this.EvaluateInternal() ?? false;
        }

        private void RebuildFunction()
        {
            try
            {
                var expression = DynamicExpressionParser.ParseLambda<DynamicConditionState, bool>(
                    new ParsingConfig() { AllowNewToEvaluateAnyType = true, ResolveTypesBySimpleName = true },
                    false,
                    this.conditionSource);
                this.func = expression.Compile();
                this.lastException = null;
            }
            catch (Exception ex)
            {
                this.lastException = $"Expression compilation failed: {ex.Message}";
                this.func = null;
            }
        }

        private void ToImGui(bool expand = true)
        {
            if (!expand)
            {
                ImGui.Text($"Expression:");
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(255, 255, 0, 255), $"{this.conditionSource.Replace("\n", " ").Trim()}");
                ImGui.SameLine();
                ImGui.Text($"(Errors {this.exceptionCounter})");
                return;
            }

            ImGui.TextWrapped("Type the expression in the following box to make custom condition.");
            if (ImGui.InputTextMultiline(
                "##dynamicConditionCode",
                ref this.conditionSource,
                10000,
                new Vector2(ImGui.GetContentRegionAvail().X / 1f, 0f)))
            {
                this.RebuildFunction();
            }

            var result = this.EvaluateInternal();
            ImGui.PushTextWrapPos(ImGui.GetContentRegionMax().X);
            if (result == null)
            {
                ImGui.TextColored(CodeCompileFailure, this.lastException);
            }
            else if (this.Evaluate())
            {
                ImGui.TextColored(ConditionSuccess, $"Condition yeilds true. " +
                    $"Exception Counter is {this.exceptionCounter}");
            }
            else
            {
                ImGui.TextColored(ConditionFailure, $"Condition yeilds false. " +
                    $"Error Counter is {this.exceptionCounter}");
            }

            ImGui.PopTextWrapPos();
        }

        /// <summary>
        /// Evaluates the condition expression and returns true, false, null (for exception).
        /// Also, increments the total number of exceptions that occurs in this condition
        /// and stores the last exception message.
        ///
        /// Example Exceptions that can occur:
        ///   0/0
        ///   Flasks[-1].Charges > 0
        ///   MaxHp/CurrentHp where CurrentHp = 0
        ///
        /// NOTE: This function hides the last exception message if the exception
        /// is automatically resolved in the next call. This is because we want to
        /// be forgiving instead of requiring the user to add a bunch of preconditions
        /// to their code snippets.
        /// </summary>
        private bool? EvaluateInternal()
        {
            bool? result = null;
            if (this.func != null)
            {
                try
                {
                    result = this.func(state.Value);
                    this.lastException = string.Empty;
                }
                catch (Exception ex)
                {
                    this.lastException = $"Exception while evaluation ({this.exceptionCounter}): {ex.Message}";
                    this.exceptionCounter++;
                }
            }

            return result;
        }
    }
}
