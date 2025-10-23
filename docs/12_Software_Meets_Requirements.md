# Software Meets Requirements — Evidence Pack

> This page proves the implemented software satisfies the SRS. See also `09_Requirements_Traceability.md`. PDF Final REPORT

## A. Demo Checklist (marker can follow)
- [x] Launch app (`Update-Database` → `dotnet run`)
- [x] **Store** lists products with search/category → `docs/evidence/01_store.png`
- [x] **Cart** adds/edits/removes items → `docs/evidence/02_added_to_cart.png`
- [x] **Checkout → Done** shows **Subtotal/Tax/Total** and clears cart → `docs/evidence/03_checkout_done.png`
- [x] **Transactional stock decrement** observed after order → `docs/evidence/04_placed_order.png`
- [x] **My Orders** shows history + lifetime **tier badge** → `docs/evidence/05_my_order.png`
- [x] **Admin → Products** shows **Price/Cost/Margin%** (screenshot to add if needed)
- [x] **Admin → Orders** list/details show **Totals/Profit** and status actions (screenshot to add if needed)
- [x] **Admin → Email Groups** Compose → Sent (simulated if no SMTP) (screenshot to add if needed)

## B. Requirement-by-Requirement Proof (condensed)
- **FR-01 Catalog** — `/Store` implemented; screenshot **01_store**.  
- **FR-02 Cart** — `CartController` + session cart; screenshot **02_added_to_cart**.  
- **FR-03 Checkout/Order** — `CheckoutController` + `CheckoutService.PlaceOrderAsync`; screenshot **03_checkout_done**.  
- **FR-04 Stock decrement (atomic)** — in service transaction; verified by product stock drop; screenshot **04_placed_order**.  
- **FR-05 Profit snapshots** — `OrderItem.UnitPrice`, `CostPriceSnapshot`; `Order.Subtotal/Tax/Total/Profit`; visible in **Admin Orders**.  
- **FR-06 Admin Products (cost/supplier/margin)** — Admin views show Margin%; screenshot to attach.  
- **FR-07 Admin Orders & Status** — Admin actions `[ValidateAntiForgeryToken]`; screenshot to attach.  
- **FR-08 Customer history + tier** — `/Orders/My`; screenshot **05_my_order**.  
- **FR-09 Email Groups** — Owner-only; simulated/SMTP via DI; screenshot to attach.

*(Where a screenshot is “to attach”, drop image(s) into `/docs/evidence` and add the filename above.)*
## note## “All product/diagram images in this coursework are **AI-generated (Google Nano Banana, 2025)**


## C. Non-Functional (key ones)
- **Setup simplicity** — .NET 8 + SQLite; proven by `01_Installation_and_Running.md`.  
- **Security baseline** — anti-forgery on POST, Razor encoding, model validation; Owner-only Admin.  
- **Reliability** — order + stock update committed in one EF Core transaction (service code).

## D. Known Limits (declared)
- Reports/CSV and full User Admin are deferred (see `11_Value_Adding.md`). Core flows are complete and demonstrated.
