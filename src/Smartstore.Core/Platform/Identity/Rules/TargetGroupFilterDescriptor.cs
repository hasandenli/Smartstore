﻿using System;
using System.Linq.Expressions;
using Smartstore.Core.Rules;
using Smartstore.Core.Rules.Filters;

namespace Smartstore.Core.Identity.Rules
{
    public class TargetGroupFilterDescriptor : FilterDescriptor<Customer, bool>
    {
        private readonly IRuleService _ruleService;
        private readonly WeakReference<IRuleVisitor> _ruleVisitor;

        public TargetGroupFilterDescriptor(IRuleService ruleService, IRuleVisitor ruleVisitor)
            : base(x => true)
        {
            _ruleService = ruleService;
            _ruleVisitor = new WeakReference<IRuleVisitor>(ruleVisitor);
        }

        public override Expression GetExpression(RuleOperator op, Expression valueExpression, bool liftToNull)
        {
            var ruleSetId = ((ConstantExpression)valueExpression).Value.Convert<int>();

            // Get other expression group.
            _ruleVisitor.TryGetTarget(out var visitor);

            // TODO: (mg) (core) TargetGroupFilterDescriptor lacks async implementations. There is never an end to this.
            var otherGroup = _ruleService.CreateExpressionGroupAsync(ruleSetId, visitor).Await() as FilterExpressionGroup;

            var otherPredicate = otherGroup?.ToPredicate(liftToNull);
            if (otherPredicate is Expression<Func<Customer, bool>> lambda)
            {
                MemberExpression = lambda;
            }

            return base.GetExpression(op, Expression.Constant(true), liftToNull);
        }
    }
}