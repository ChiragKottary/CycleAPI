using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Models.Enums;
using CycleAPI.Repositories.Interface;
using CycleAPI.Service.Interface;

namespace CycleAPI.Service.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICycleRepository _cycleRepository;
        private readonly ICartService _cartService;

        public OrderService(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            ICycleRepository cycleRepository,
            ICartService cartService)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _cycleRepository = cycleRepository;
            _cartService = cartService;
        }

        public async Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(Guid customerId)
        {
            var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
            return orders.Select(MapToOrderDto);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return order != null ? MapToOrderDto(order) : null;
        }

        public async Task<OrderDto?> GetOrderByOrderNumberAsync(string orderNumber)
        {
            var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            return order != null ? MapToOrderDto(order) : null;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            // Input validation
            if (createOrderDto == null)
                throw new ArgumentNullException(nameof(createOrderDto));
                
            if (createOrderDto.OrderItems == null || !createOrderDto.OrderItems.Any())
                throw new ArgumentException("Order must contain at least one item");

            // Validate customer exists
            var customer = await _customerRepository.GetCustomerByIdAsync(createOrderDto.CustomerId);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID {createOrderDto.CustomerId} not found");

            // Validate shipping information
            if (string.IsNullOrWhiteSpace(createOrderDto.ShippingAddress))
                throw new ArgumentException("Shipping address is required");
            if (string.IsNullOrWhiteSpace(createOrderDto.ShippingCity))
                throw new ArgumentException("Shipping city is required");
            if (string.IsNullOrWhiteSpace(createOrderDto.ShippingState))
                throw new ArgumentException("Shipping state is required");
            if (string.IsNullOrWhiteSpace(createOrderDto.ShippingPostalCode))
                throw new ArgumentException("Shipping postal code is required");

            // Generate unique order number
            var orderNumber = GenerateOrderNumber();

            // Create new order
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerId = createOrderDto.CustomerId,
                OrderNumber = orderNumber,
                Status = OrderStatus.Pending,
                ShippingAddress = createOrderDto.ShippingAddress.Trim(),
                ShippingCity = createOrderDto.ShippingCity.Trim(),
                ShippingState = createOrderDto.ShippingState.Trim(),
                ShippingPostalCode = createOrderDto.ShippingPostalCode.Trim(),
                Notes = createOrderDto.Notes?.Trim(),
                OrderDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            try
            {
                // Process order items
                decimal totalAmount = 0;
                foreach (var itemDto in createOrderDto.OrderItems)
                {
                    if (itemDto.Quantity <= 0)
                        throw new ArgumentException($"Invalid quantity ({itemDto.Quantity}) for cycle ID {itemDto.CycleId}");

                    var cycle = await _cycleRepository.GetByIdAsync(itemDto.CycleId);
                    if (cycle == null)
                        throw new KeyNotFoundException($"Cycle with ID {itemDto.CycleId} not found");

                    if (!cycle.IsActive)
                        throw new InvalidOperationException($"Cycle {cycle.ModelName} is not currently available for purchase");

                    if (cycle.StockQuantity < itemDto.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for cycle {cycle.ModelName}. Requested: {itemDto.Quantity}, Available: {cycle.StockQuantity}");

                    var orderItem = new OrderItem
                    {
                        OrderItemId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        CycleId = cycle.CycleId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = cycle.Price,
                        Subtotal = cycle.Price * itemDto.Quantity,
                        Notes = itemDto.Notes?.Trim(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    totalAmount += orderItem.Subtotal;
                    order.OrderItems.Add(orderItem);

                    // Update stock quantity
                    cycle.StockQuantity -= itemDto.Quantity;
                    await _cycleRepository.UpdateAsync(cycle);
                }

                order.TotalAmount = totalAmount;

                // Save order - transaction is handled inside CreateOrderAsync
                var createdOrder = await _orderRepository.CreateOrderAsync(order);

                return await GetOrderByIdAsync(createdOrder.OrderId) 
                    ?? throw new Exception("Failed to retrieve created order");
            }
            catch
            {
                // Transaction rollback is handled inside CreateOrderAsync if an error occurs
                throw;
            }
        }

        public async Task<OrderDto> CreateOrderFromCartAsync(CreateOrderFromCartDto createOrderDto)
        {
            if (createOrderDto == null)
                throw new ArgumentNullException(nameof(createOrderDto));

            var cart = await _cartService.GetCartByIdAsync(createOrderDto.CartId);
            if (cart == null)
                throw new KeyNotFoundException($"Cart with ID {createOrderDto.CartId} not found");

            if (!cart.IsActive)
                throw new InvalidOperationException("Cannot create order from inactive cart");

            if (cart.CartItems == null || !cart.CartItems.Any())
                throw new InvalidOperationException("Cannot create order from empty cart");

            // Validate shipping information
            if (string.IsNullOrWhiteSpace(createOrderDto.ShippingAddress))
                throw new ArgumentException("Shipping address is required");
            if (string.IsNullOrWhiteSpace(createOrderDto.ShippingCity))
                throw new ArgumentException("Shipping city is required");
            if (string.IsNullOrWhiteSpace(createOrderDto.ShippingState))
                throw new ArgumentException("Shipping state is required");
            if (string.IsNullOrWhiteSpace(createOrderDto.ShippingPostalCode))
                throw new ArgumentException("Shipping postal code is required");

            // Generate unique order number
            var orderNumber = GenerateOrderNumber();

            // Create new order
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerId = cart.CustomerId,
                OrderNumber = orderNumber,
                Status = OrderStatus.Pending,
                ShippingAddress = createOrderDto.ShippingAddress.Trim(),
                ShippingCity = createOrderDto.ShippingCity.Trim(),
                ShippingState = createOrderDto.ShippingState.Trim(),
                ShippingPostalCode = createOrderDto.ShippingPostalCode.Trim(),
                Notes = createOrderDto.Notes?.Trim(),
                OrderDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            decimal totalAmount = 0;

            // Convert cart items to order items
            foreach (var cartItem in cart.CartItems)
            {
                var cycle = await _cycleRepository.GetByIdAsync(cartItem.CycleId);
                if (cycle == null)
                    throw new KeyNotFoundException($"Cycle with ID {cartItem.CycleId} not found");

                if (!cycle.IsActive)
                    throw new InvalidOperationException($"Cycle {cycle.ModelName} is not currently available for purchase");

                if (cycle.StockQuantity < cartItem.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for cycle {cycle.ModelName}. Requested: {cartItem.Quantity}, Available: {cycle.StockQuantity}");

                var orderItem = new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    CycleId = cycle.CycleId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cycle.Price,
                    Subtotal = cycle.Price * cartItem.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                totalAmount += orderItem.Subtotal;
                order.OrderItems.Add(orderItem);

                // Update stock quantity
                cycle.StockQuantity -= cartItem.Quantity;
                await _cycleRepository.UpdateAsync(cycle);
            }

            order.TotalAmount = totalAmount;

            // Save order
            var createdOrder = await _orderRepository.CreateOrderAsync(order);

            // Clear the cart after successful order creation
            await _cartService.ClearCartAsync(cart.CartId);

            return await GetOrderByIdAsync(createdOrder.OrderId) 
                ?? throw new Exception("Failed to retrieve created order");
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(int page = 1, int pageSize = 50)
        {
            var (orders, _) = await _orderRepository.GetFilteredAsync(new OrderQueryParameters 
            { 
                Page = page,
                PageSize = pageSize 
            });
            return orders.Select(MapToOrderDto);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status)
        {
            var (orders, _) = await _orderRepository.GetFilteredAsync(new OrderQueryParameters 
            { 
                Status = status,
                Page = 1,
                PageSize = int.MaxValue 
            });
            return orders.Select(MapToOrderDto);
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, Guid? processedByUserId = null)
        {
            try
            {
                // First check if order exists using the simpler ExistsAsync method
                if (!await _orderRepository.ExistsAsync(orderId))
                    return false;

                // Use direct status update method from repository instead of full entity update
                return await _orderRepository.UpdateStatusAsync(orderId, status);
            }
            catch (Exception ex)
            {
                // Log the error but don't throw it up the stack
                Console.WriteLine($"Error updating order status: {ex.Message}");
                return false;
            }
        }

        public async Task<PagedResult<OrderDto>> GetFilteredOrdersAsync(OrderQueryParameters parameters)
        {
            var result = await _orderRepository.GetFilteredAsync(parameters);
            
            return new PagedResult<OrderDto>
            {
                Items = result.Orders.Select(MapToOrderDto),
                TotalItems = result.TotalCount,
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize
            };
        }

        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }

        private static OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}",
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                ShippingCity = order.ShippingCity,
                ShippingState = order.ShippingState,
                ShippingPostalCode = order.ShippingPostalCode,
                Notes = order.Notes,
                ProcessedByUserId = order.ProcessedByUserId,
                ProcessedByUserName = order.ProcessedByUser != null 
                    ? $"{order.ProcessedByUser.FirstName} {order.ProcessedByUser.LastName}"
                    : null,
                OrderDate = order.OrderDate,
                ProcessedDate = order.ProcessedDate,
                ShippedDate = order.ShippedDate,
                DeliveredDate = order.DeliveredDate,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderItems = order.OrderItems.Select(MapToOrderItemDto).ToList()
            };
        }

        private static OrderItemDto MapToOrderItemDto(OrderItem item)
        {
            return new OrderItemDto
            {
                OrderItemId = item.OrderItemId,
                OrderId = item.OrderId,
                CycleId = item.CycleId,
                CycleName = item.Cycle.ModelName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Subtotal = item.Subtotal,
                Notes = item.Notes,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };
        }

        public async Task<bool> ExistsAsync(Guid orderId)
        {
            return await _orderRepository.ExistsAsync(orderId);
        }
    }
}