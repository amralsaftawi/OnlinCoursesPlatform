# 🎓 Online Courses Platform

![.NET](https://img.shields.io/badge/.NET-ASP.NET_Core-blue)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-brightgreen)
![Status](https://img.shields.io/badge/Status-Active-success)
![License](https://img.shields.io/badge/License-MIT-lightgrey)

A scalable full-stack web application that enables instructors to create, manage, and deliver courses, while allowing students to discover, enroll, and consume educational content seamlessly.

---

## 🚀 Overview

**Online Courses Platform** simulates a real-world e-learning system with role-based access:

* 👨‍🎓 Students
* 👨‍🏫 Instructors

The system is designed with a strong focus on:

* Clean Architecture
* Separation of Concerns
* Maintainability & Scalability

---

## 🎯 Demo Idea (Optional)

> You can deploy this later and add:

```
Live Demo: https://your-demo-link.com
```

---

## ✨ Features

### 👨‍🎓 Student

* Browse courses with search & filtering
* Enroll in courses
* Watch video lessons
* Read article-based lessons (file upload support)
* Track learning progress

### 👨‍🏫 Instructor

* Create, edit, and delete courses
* Add lessons:

  * 🎥 Video (URL-based)
  * 📄 Article (File Upload)
* Organize course structure (Sections & Lessons)
* Manage content بسهولة

### ⚙️ System Features

* Authentication & Authorization
* Validation & error handling
* Modular and scalable design
* Clean separation between layers

---

## 🏗️ Architecture

The application follows **Clean Architecture (N-Tier):**

```id="arch1"
Presentation Layer   → Controllers / Views  
Application Layer    → Services / DTOs  
Domain Layer         → Entities  
Infrastructure Layer → Database / File System  
```

### 🔍 Why this matters:

* Loose coupling
* Easier testing
* Scalable codebase
* Industry-standard structure

---

## 🛠️ Tech Stack

| Layer        | Technology                 |
| ------------ | -------------------------- |
| Backend      | ASP.NET Core MVC           |
| Frontend     | HTML, CSS, JavaScript      |
| Database     | SQL Server                 |
| Architecture | Clean Architecture         |
| Tools        | LINQ, Dependency Injection |

---

## 📂 Project Structure

```id="struct1"
/Controllers      → MVC Controllers  
/Services         → Business Logic  
/DTOs             → Data Transfer Objects  
/Models           → Entities  
/Views            → Razor Views  
/wwwroot          → Static Assets  
```

---

## ⚡ Key Concepts Implemented

* DTO Pattern (decoupling data layers)
* Dependency Injection (DI)
* File Upload System (Article Lessons)
* Dynamic Rendering (Video vs Article)
* Clean Code Principles

---

## ▶️ Getting Started

### 1️⃣ Clone the repository

```bash id="cmd1"
git clone https://github.com/amralsaftawi/OnlinCoursesPlatform.git
cd OnlinCoursesPlatform
```

---

### 2️⃣ Configure Database

Update connection string in:

```
appsettings.json
```

ثم شغل:

```bash id="cmd2"
dotnet ef database update
```

---

### 3️⃣ Run the project

```bash id="cmd3"
dotnet run
```

---

## 📸 Screenshots


Suggested:

* Home Page
* Course Details
* Lesson View (Video / Article)
* Instructor Dashboard

---

## 🧠 What This Project Demonstrates

* Real-world backend architecture
* Handling different content types (Video / File)
* Building scalable MVC applications
* Clean separation between layers
* Practical system design thinking

---

## 🔮 Future Improvements

* 🧪 Quizzes & Exams
* 💳 Payment Integration
* ⭐ Ratings & Reviews
* 💬 Real-time Chat
* 🎥 Video Hosting بدل URL

---

## 🤝 Contributing

Contributions are welcome!

```bash id="flow1"
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
