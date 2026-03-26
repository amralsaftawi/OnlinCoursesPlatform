# 🎓 Online Courses Platform

![.NET](https://img.shields.io/badge/.NET-ASP.NET_Core-blue)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-brightgreen)
![Status](https://img.shields.io/badge/Status-Active-success)

A full-stack e-learning platform that supports **Students, Instructors, and Admins**, enabling course creation, management, and controlled content delivery in a scalable and maintainable architecture.

---

## 🚀 Overview

This project simulates a real-world **Learning Management System (LMS)** with **three distinct roles**:

* 👨‍🎓 Student → consumes content
* 👨‍🏫 Instructor → creates content
* 🛡️ Admin → manages and controls the system

The system is designed using **Clean Architecture principles** to ensure scalability, maintainability, and clear separation of responsibilities.

---

## 👥 Roles & Permissions

### 👨‍🎓 Student

* Browse courses with search & filtering
* Enroll in courses
* Access course content
* Watch video lessons
* Read article-based lessons (uploaded files)

---

### 👨‍🏫 Instructor

* Create and manage courses
* Add structured content (Sections & Lessons)
* Upload lessons:

  * 🎥 Video (via URL)
  * 📄 Article (file upload)
* Update or delete course content

---

### 🛡️ Admin

* Manage users (Students & Instructors)
* Control platform content
* Monitor and moderate courses
* Ensure system integrity and access control

---

## ✨ Core Features

* Role-based Authorization (Student / Instructor / Admin)
* Dynamic lesson handling (Video vs Article)
* File upload system for article lessons
* Clean separation between layers (DTOs, Services, Controllers)
* Validation & error handling
* Scalable and modular design

---

## 🏗️ Architecture

The application follows **Clean Architecture (N-Tier)**:

```id="arch2"
Presentation Layer   → Controllers / Views  
Application Layer    → Services / DTOs  
Domain Layer         → Entities  
Infrastructure Layer → Database / File Handling  
```

### Why this matters:

* Decoupled components
* Easier testing & debugging
* Production-ready structure

---

## 🛠️ Tech Stack

| Layer        | Technology            |
| ------------ | --------------------- |
| Backend      | ASP.NET Core MVC      |
| Frontend     | HTML, CSS, JavaScript |
| Database     | SQL Server            |
| Architecture | Clean Architecture    |
| Concepts     | DI, LINQ, DTO Pattern |

---

## 📂 Project Structure

```id="struct2"
/Controllers      → Handles HTTP requests  
/Services         → Business logic layer  
/DTOs             → Data transfer objects  
/Models           → Core entities  
/Views            → Razor UI  
/wwwroot          → Static assets  
```

---

## ⚡ Key Engineering Concepts

* Role-Based Access Control (RBAC)
* DTO vs ViewModel separation
* Dependency Injection (DI)
* File Handling (Upload & Rendering)
* Dynamic content rendering
* Clean code & modular design

---

## ▶️ Getting Started

### 1️⃣ Clone the repository

```bash id="cmd11"
git clone https://github.com/amralsaftawi/OnlinCoursesPlatform.git
cd OnlinCoursesPlatform
```

---

### 2️⃣ Configure Database

Update:

```id="cfg1"
appsettings.json
```

Then run:

```bash id="cmd22"
dotnet ef database update
```

---

### 3️⃣ Run the project

```bash id="cmd33"
dotnet run
```

---

## 📸 Screenshots

> Add real screenshots here to showcase:

* Home Page
* Course Details
* Lesson View (Video / Article)
* Instructor Dashboard
* Admin Panel

---

## 🔮 Future Improvements

* 🧪 Quizzes & Exams
* 💳 Payment System
* ⭐ Ratings & Reviews
* 💬 Real-time Chat
* 📊 Admin Analytics Dashboard
* 🎥 Internal video hosting

---

## 🤝 Contributing

```bash id="flow2"
1. Fork the repo
2. Create your branch
3. Commit your changes
4. Open a Pull Request
```

---

## 👨‍💻 Authors

* **Amr Alsaftawi**
  https://github.com/amralsaftawi

* **Ahmed Aldsoaky**
  https://github.com/ahmedaldsoaky

* **Mohamed Elsheikh**
  https://github.com/MohamedElsheikh17

---

## ⭐ Final Notes

This project demonstrates:

* Building a real-world LMS system
* Handling multi-role architecture
* Applying clean architecture in ASP.NET Core
* Designing scalable backend systems

It represents a strong step toward production-level backend development.
