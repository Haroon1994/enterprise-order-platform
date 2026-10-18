namespace EnterpriseOrderPlatform.Modules.Ordering.Contracts;

public sealed record SetCartItemRequest(int Quantity);

public sealed record CartItemResponse(string Sku, int Quantity);

public sealed record CartResponse(IReadOnlyList<CartItemResponse> Items);
