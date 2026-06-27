namespace CMS.Data.Enums
{
    public enum OrderItemStatus
    {
        Normal = 0,
        Preparing = 1,
        Shortage = 2,
        Damaged = 3,
        AwaitingCustomer = 4,
        QuantityAdjusted = 5,
        Removed = 6,
        Cancelled = 7,
        ReadyToShip = 8,
        Shipped = 9,
        Completed = 10
    }

    public enum OrderItemIssueType
    {
        Shortage = 0,
        Damaged = 1,
        Lost = 2,
        QualityFailed = 3,
        Other = 4
    }

    public enum OrderItemIssueStatus
    {
        Open = 0,
        WaitingForCustomer = 1,
        CustomerAcceptedAdjustment = 2,
        WaitingForRestock = 3,
        ItemRemoved = 4,
        OrderCancelled = 5,
        Resolved = 6
    }

    public enum CustomerIssueDecision
    {
        AcceptReducedQuantity = 0,
        RemoveItem = 1,
        WaitForRestock = 2,
        CancelEntireOrder = 3
    }
}
