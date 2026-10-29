# Demo: end-to-end checkout

Start the API (see README), then:

```bash
B=http://localhost:5080
# 1. Register and log in
curl -X POST $B/api/identity/register -H "Content-Type: application/json" -d '{"email":"demo@example.com","password":"correct-horse-battery"}'
TOKEN=$(curl -s -X POST $B/api/identity/login -H "Content-Type: application/json" -d '{"email":"demo@example.com","password":"correct-horse-battery"}' | sed -n 's/.*"accessToken":"\([^"]*\)".*/\1/p')

# 2. Add to cart (a product and stock must exist; create them as Admin/Warehouse)
curl -X PUT $B/api/cart/items/E2E-1 -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{"quantity":2}'

# 3. Check out (retrying with the same key never creates a second order)
curl -X POST $B/api/orders/checkout -H "Authorization: Bearer $TOKEN" -H "Idempotency-Key: demo-1"
```

Admin and Warehouse roles are not assignable through the API on purpose. For a local demo, insert them directly into `identity."AspNetUserRoles"`.
