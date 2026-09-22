namespace Utils
{
    public enum OrderProcessStep
    {
        PaymentCompleted = 1,
        OrderReceived = PaymentCompleted,
        ProductPreparing = 2,
        Preparing = ProductPreparing,
        DeliveryPreparing = 3,
        OutForDelivery = 4,
        Delivered = 5
    }
}
