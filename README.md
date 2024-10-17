## Task: Development of a service that manages courier operations

### Goal: 
Develop a new REST API service that will register couriers, add new orders, and assign them to couriers.

### Service Requirements
The service must implement the following:

- REST API — details in Task 1;
- courier rating calculation — details in Task 2;
- rate limiter for the service — details in Task 3;
- algorithm for assigning orders to couriers — details in Task 4.

### Task 1: REST API
As the base functionality of the service, you need to implement 7 basic methods.

For all methods, if the response is successful, an HTTP 200 OK status is expected.

#### POST /couriers
The interface for uploading a list of couriers to the system is described below.

The handler receives a list in JSON format with courier data and their work schedule.

Couriers only work in predefined regions and are distinguished by type: on foot, bicycle courier, and car courier. The type of courier affects the volume of orders they can carry. Regions are represented by positive integers. The work schedule is provided as a list of strings in the format `HH:MM-HH:MM`.

#### GET /couriers/{courier_id}
Returns information about a specific courier.

#### GET /couriers
Returns information about all couriers.

The method has `offset` and `limit` parameters for pagination. If:

- `offset` or `limit` are not provided, assume `offset = 0` and `limit = 1`;
- if no records are found for the given `offset` and `limit`, return an empty list of couriers.

#### POST /orders
Accepts a list of orders in JSON format. Each order has characteristics such as weight, region, delivery time, and price. Delivery time is a string in the format `HH:MM-HH:MM`, where `HH` is the hour (from 0 to 23) and `MM` is the minute (from 0 to 59). Examples: `"09:00-11:00"`, `"12:00-23:00"`, `"00:00-23:59"`.

#### GET /orders/{order_id}
Returns information about an order by its ID, including additional details like order weight, delivery region, and time slots suitable for receiving the order.

#### GET /orders
Returns information about all orders, including additional details like order weight, delivery region, and time slots for receiving the order.

The method has `offset` and `limit` parameters for pagination. If:

- `offset` or `limit` are not provided, assume `offset = 0` and `limit = 1`;
- if no records are found for the given `offset` and `limit`, return an empty list of orders.

#### POST /orders/complete
Accepts an array of objects consisting of three fields: courier ID, order ID, and completion time. It then marks the order as completed.

If the order:

- is not found, was assigned to another courier, or was not assigned at all — return an HTTP 400 Bad Request;
- is successfully completed — return HTTP 200 OK and the ID of the completed order.

The handler must be idempotent.

### Task 2: Courier Rating
The service team decided to start tracking couriers' earnings and ratings. To achieve this, a new method `GET /couriers/meta-info/{courier_id}` needs to be implemented.

Method parameters:

- `start_date` — the start date for rating calculations;
- `end_date` — the end date for rating calculations.

An example parameter value could be `2023-01-20`. You can assume that all orders and dates for calculations are in the same fixed time zone — UTC.

The method should return the courier's earnings for the orders and their rating.

#### Earnings Calculation:
Earnings are calculated as the sum of payments for each completed delivery from `start_date` (inclusive) to `end_date` (exclusive):

sum = ∑(cost * C)

where `C` is a coefficient that depends on the courier type:

- on foot — 2;
- bicycle courier — 3;
- car courier — 4.

If the courier has not completed any deliveries, earnings should not be calculated or returned.

#### Rating Calculation:
The rating is calculated as follows:

rating = ((number of all completed orders from start_date to end_date) / (number of hours between start_date and end_date)) * C


Where `C` is a coefficient that depends on the courier type:

- on foot = 3;
- bicycle courier = 2;
- car courier = 1.

If the courier has not completed any deliveries, the rating should not be calculated or returned.

### Task 3: Rate Limiter
Any large service with an API accessible from the internet should limit the number of incoming requests. To achieve this, a rate limiter needs to be implemented for the service.

You need to implement a solution that limits the load to **10 RPS** for each endpoint. If the allowed number of requests is exceeded, the service should respond with HTTP status code 429.

### Task 4: Order Assignment
Currently, the service assigns one order per courier. This can lead to underutilized couriers or delivery to remote locations. Before the start of each working shift, the service should assign orders to couriers in a way that minimizes delivery costs.

To implement this, the following methods are required:

- **POST /orders/assign**: This method will assign available orders to couriers before the start of the workday.
- **GET /couriers/assignments**: This method will return already assigned orders.

Order assignment takes into account the following parameters:

- order weight;
- delivery region;
- delivery cost.

#### Order Weight:
Each category of courier has a weight limit for the orders they can carry, as well as a maximum number of orders.

| Courier Type | Max Weight | Max Number of Orders |
|--------------|------------|----------------------|
| On foot      | 10 kg      | 2                    |
| Bicycle      | 20 kg      | 4                    |
| Car          | 40 kg      | 7                    |

#### Delivery Region:
The type of transport determines the number of regions the courier can deliver to.

| Courier Type | Number of Regions | Comment                               |
|--------------|-------------------|---------------------------------------|
| On foot      | 1                 | Delivery is only possible in one region |
| Bicycle      | 2                 | Delivery can occur in two regions      |
| Car          | 3                 | Delivery can occur in three regions    |

#### Delivery Time:
Delivery time consists of the time required to visit all delivery points in the region and the waiting time for handing over the order.

Time to visit all points in one region:

| Courier Type | 1st Order | Subsequent Orders |
|--------------|-----------|-------------------|
| On foot      | 25 min    | 10 min            |
| Bicycle      | 12 min    | 8 min             |
| Car          | 8 min     | 4 min             |

For deliveries to another region, the time is calculated similarly:

| Courier Type | 1st Order | Subsequent Orders |
|--------------|-----------|-------------------|
| Bicycle      | 12 min    | 8 min             |
| Car          | 8 min     | 4 min             |

Delivery time is limited to the working hours. For example, if a courier works from 10:00 to 12:00 without transport, they can deliver up to 4 orders without combining:

| Time  | Order Number |
|-------|--------------|
| 10:00 | 1            |
| 10:25 | 2            |
| 10:50 | 3            |
| 11:15 | 4            |

With order combining, more orders can be delivered in the same time:

| Time  | Order Number |
|-------|--------------|
| 10:00 | [1, 2]       |
| 10:35 | [3, 4]       |
| 11:10 | [5, 6]       |
| 11:45 | [7, 8]       |

#### Delivery Cost:
The cost of delivery with order grouping is calculated as follows:

| Courier Type | 1st Order | Subsequent Orders |
|--------------|-----------|-------------------|
| On foot      | 100%      | 80%               |
| Bicycle      | 100%      | 80%               |
| Car          | 100%      | 80%               |
