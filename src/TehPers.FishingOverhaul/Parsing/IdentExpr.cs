﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace TehPers.FishingOverhaul.Parsing
{
    internal record IdentExpr(string Name) : Expr<double>
    {
        public override bool TryEvaluate(
            IDictionary<string, double> variables,
            HashSet<string> missingVariables,
            out double result
        )
        {
            if (!variables.TryGetValue(this.Name, out var value))
            {
                result = default;
                missingVariables.Add(this.Name);
                return false;
            }

            result = value;
            return true;
        }

        public override bool TryCompile(
            Dictionary<string, ParameterExpression> variables,
            HashSet<string> missingVariables,
            [MaybeNullWhen(false)] out Expression result
        )
        {
            if (!variables.TryGetValue(this.Name, out var param))
            {
                result = default;
                return false;
            }

            result = param;
            return true;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
