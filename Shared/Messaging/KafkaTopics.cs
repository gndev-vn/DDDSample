namespace Shared.Messaging;

public static class KafkaTopics
{
    public static class Catalog
    {
        public const string CategoryCreated = "catalog.category.created";
        public const string CategoryUpdated = "catalog.category.updated";
        public const string CategoryDeleted = "catalog.category.deleted";
        public const string ProductCreated = "catalog.product.created";
        public const string ProductUpdated = "catalog.product.updated";
        public const string ProductDeleted = "catalog.product.deleted";
        public const string ProductVariantCreated = "catalog.product-variant.created";
        public const string ProductVariantUpdated = "catalog.product-variant.updated";
        public const string ProductVariantDeleted = "catalog.product-variant.deleted";

        public static readonly string[] OrderingProjectionTopics =
        [
            CategoryCreated,
            CategoryUpdated,
            CategoryDeleted,
            ProductCreated,
            ProductUpdated,
            ProductDeleted,
            ProductVariantCreated,
            ProductVariantUpdated,
            ProductVariantDeleted
        ];
    }

    public static class Ordering
    {
        public const string OrderCreated = "ordering.order.created";
        public const string OrderUpdated = "ordering.order.updated";
        public const string OrderCanceled = "ordering.order.canceled";
        public const string OrderPaid = "ordering.order.paid";
        public const string OrderLineAdded = "ordering.order.line-added";

        public static readonly string[] PaymentWorkflowTopics =
        [
            OrderCreated,
            OrderUpdated,
            OrderCanceled
        ];
    }

    public static class Payment
    {
        public const string PaymentCreated = "payment.created";
        public const string PaymentCompleted = "payment.completed";
        public const string PaymentFailed = "payment.failed";

        public static readonly string[] OrderingWorkflowTopics =
        [
            PaymentCreated,
            PaymentCompleted,
            PaymentFailed
        ];
    }
}
