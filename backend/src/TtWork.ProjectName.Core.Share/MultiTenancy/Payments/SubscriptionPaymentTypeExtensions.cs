﻿ namespace TtWork.ProjectName.Core.Share.MultiTenancy.Payments
{
    public static class SubscriptionPaymentTypeExtensions
    {
        public static bool IsRecurring(this SubscriptionPaymentType subscriptionPaymentType)
        {
            return subscriptionPaymentType != SubscriptionPaymentType.Manual;
        }
    }
}