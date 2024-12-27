# Inno_Shop

## 📖 Introduction
**Inno_Shop** is a modern e-commerce platform built using a microservices architecture. The project consists of two key microservices: **User Management** and **Product Management**. These services are developed with ASP.NET Core and communicate via RESTful APIs, ensuring scalability and modularity.

## 🚀 Main Features

### 🧑‍💼 User Management Microservice
- Provides a RESTful API for managing users (CRUD operations).
- Key attributes: **ID**, **name**, **email**, **role**, etc.
- Implements robust authentication and authorization using JWT tokens.
- Features:
  - Password recovery functionality.
  - Email-based account verification.
  - Deactivation of users, which hides their products (supports reactivation via SoftDelete).

### 🛒 Product Management Microservice
- Offers a RESTful API for product operations (create, read, update, delete).
- Key attributes: **ID**, **name**, **description**, **price**, **availability**, **creator user ID**, **creation date**, etc.
- Enhanced functionality:
  - Searching and filtering based on various criteria.
  - Secure access: only authenticated users can add, delete, or edit products.
  - Users can manage only their own products.

## ⚙️ Technical Details

### 🛠 Technologies
- **Backend**: ASP.NET Core.
- **Authentication**: JSON Web Tokens (JWT).
- **Database**: PostgreSQL or SQL Server, with Entity Framework Code-First approach.
- **Deployment**: Docker/Docker Compose.
- **Testing**: Comprehensive unit and integration tests for high-quality assurance.

### 🐳 Deployment with Docker Compose
1. Ensure you have Docker and Docker Compose installed on your system.
2. Clone the repository:
   ```bash
   git clone https://github.com/your-repository/inno_shop.git
   ```
3. Navigate to the project directory:
   ```bash
   cd inno_shop
   ```
4. Create a `.env` file in the root directory and replace the placeholders with your own values:
   ```env
   USER_SERVICE_PORT=5000
   PRODUCT_SERVICE_PORT=5001
   POSTGRES_PORT=5432
   POSTGRES_USER=<your_postgres_user>
   POSTGRES_PASSWORD=<your_postgres_password>
   POSTGRES_DB=<your_postgres_database>

   PRODUCT_POSTGRES_PORT=5433
   PRODUCT_POSTGRES_USER=<your_product_postgres_user>
   PRODUCT_POSTGRES_PASSWORD=<your_product_postgres_password>
   PRODUCT_POSTGRES_DB=<your_product_postgres_database>
   ```
   > Replace `<your_postgres_user>`, `<your_postgres_password>`, and other placeholders with your specific configurations.

5. Build and start the services:
   ```bash
   docker-compose up --build
   ```
6. Access the services:
   - **User Management API**: `http://localhost:<USER_SERVICE_PORT>`
   - **Product Management API**: `http://localhost:<PRODUCT_SERVICE_PORT>`

7. Stop the services:
   ```bash
   docker-compose down
   ```

### 🧪 Testing
#### Unit Tests
- **Purpose**: Validate individual components (e.g., controllers, services, and models).
- **Frameworks**: NUnit, Moq (for mocking dependencies).
- **Example Test Case**:
  - Verifying the behavior of the user registration endpoint.
  - Mocking dependencies for the database layer and ensuring isolated testing.

#### Integration Tests
- **Purpose**: Ensure the services interact correctly with external systems like databases or other APIs.
- **Frameworks**: ASP.NET Core TestServer, Postman/Newman.
- **Key Scenarios**:
  - User authentication and token generation.
  - Product filtering and search endpoints.

#### Running Tests
- Navigate to the microservice directory:
  ```bash
  cd <microservice-folder>
  ```
- Execute tests:
  ```bash
  dotnet test
  ```

## 📂 Project Structure
The project follows the **Clean Architecture** principles, ensuring separation of concerns and maintainability:
- **Domain**: Core business logic and entities.
- **Application**: Use cases and service interfaces.
- **Infrastructure**: Database access, external API integrations.
- **Presentation**: API controllers and request handling.

## 📜 License
This project is licensed under the MIT License. For details, see the `LICENSE` file.

---

