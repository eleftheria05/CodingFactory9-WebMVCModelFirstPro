# CodingFactory9 Web MVC Model First Project

An ASP.NET MVC web application built using the Model-First approach with Entity Framework.

This project demonstrates the fundamentals of:

* ASP.NET MVC architecture
* Entity Framework database modeling
* CRUD operations
* Controllers and Views
* Database connectivity
* Razor pages

The repository is intended for educational purposes and helps students understand how MVC applications interact with databases using the Model-First workflow.

---

# 📚 Project Overview

The application follows the MVC (Model-View-Controller) pattern:

* **Models** handle the data and database logic.
* **Views** display the UI to the user.
* **Controllers** process requests and connect Models with Views.

The project uses:

* ASP.NET MVC
* Entity Framework
* SQL Server
* Razor View Engine

---

# ⚙️ Technologies Used

* C#
* ASP.NET MVC
* Entity Framework
* SQL Server
* Razor
* HTML5
* CSS3
* JavaScript

---

# ▶️ How to Run

## Requirements

Make sure you have installed:

* Visual Studio
* SQL Server
* .NET Framework

---

## Steps

### 1. Clone the repository

```bash
git clone https://github.com/USERNAME/REPOSITORY.git
```

### 2. Open the solution

Open the `.sln` file using Visual Studio.

---

### 3. Restore NuGet packages

In Visual Studio:

```text
Tools → NuGet Package Manager → Restore Packages
```

---

### 4. Configure database connection

Edit the `Web.config` file and update the connection string according to your SQL Server configuration.

Example:

```xml
<connectionStrings>
  <add name="DefaultConnection"
       connectionString="Server=.;Database=MyDatabase;Trusted_Connection=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

---

### 5. Run the application

Press:

```text
F5
```

or click:

```text
Start Debugging
```

---

# 🎯 Learning Goals

By studying this project, you will learn:

* MVC architecture fundamentals
* Separation of concerns
* Database-first vs model-first concepts
* Entity Framework basics
* CRUD functionality
* Routing and controllers
* Razor syntax
* SQL Server integration

---

# 🚀 Future Improvements

Possible enhancements:

* Add authentication & authorization
* Add responsive UI with Bootstrap
* Add validation
* Add API endpoints
* Improve UI/UX
* Add repository pattern
* Add dependency injection
