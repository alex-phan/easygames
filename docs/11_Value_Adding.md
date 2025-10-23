# Value Adding (Post-Submission Ideas)Please see Final report slightly diferent

These items are explicitly scheduled for after submission now. They are low-risk, high-value enhancements.

## 1) Tier Helper Service (consistency)
Purpose: centralise tier logic so badges are consistent across pages.
Sketch:
- Thresholds (constants), e.g. Bronze < $100, Silver < $500, Gold < $2000, else Platinum.
- Method: `GetTier(decimal lifetimeProfit) -> Tier`.

## 2) Orders CSV Export (evidence + analytics)
Purpose: quick data export for bookkeeping.
Sketch:
- Admin button on Orders list: “Export CSV”.
- Columns: Date, OrderId, Items, Subtotal, Tax, Total, Profit.

## 3) Simple Sales Report (owner insight)
Purpose: at-a-glance performance.
Sketch:
- Admin page “Reports”: totals by day/week/month, and top 5 products by profit (optional).

## 4) Minor UX polish
- Pagination on Store.
- Badge legend on /Orders/My.

## 5) Stretch (future)
- Email Groups by Tier once tier service is finalised.
- CSV export for Products and Users.

**Note:** These do not change core flows. They can be implemented incrementally after v1.0.0.
