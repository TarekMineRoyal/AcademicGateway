# 🎓 AcademicGateway

> Central documentation, system architecture blueprints, and ecosystem hub for the **AcademicGateway** platform.

AcademicGateway is an intelligent academic matchmaking and collaboration platform designed to connect students, professors, project blueprints, and technical skill sets using AI-driven semantic vector search and real-time relational workflows.

---

## 🧩 Sub-Repository Ecosystem

The AcademicGateway platform is split across three dedicated micro-repositories:

| Repository | Role | Tech Stack |
| :--- | :--- | :--- |
| 🗄️ **[AcademicGateway-Backend](https://github.com/TarekMineRoyal/AcademicGateway-Backend)** | Core API, Auth, Relational State & Business Workflows | Express / Node.js / PostgreSQL |
| 🎨 **[AcademicGateway-Frontend](https://github.com/TarekMineRoyal/AcademicGateway-Frontend)** | User Web Application & UI Client | React / TypeScript |
| 🤖 **[AcademicGateway-AI](https://github.com/TarekMineRoyal/AcademicGateway-AI)** | Vector Matchmaking & Semantic Search Microservice | FastAPI / LanceDB / PyTorch |

---

## 📐 High-Level Architecture

AcademicGateway follows a decoupled microservice architecture separating core business logic from computationally intensive AI tasks:

```text
                  +-------------------------+
                  |                         |
                  | AcademicGateway-Frontend|
                  |     (React / Client)    |
                  +------------+------------+
                               |
                               | REST Requests
                               v
                  +-------------------------+
                  |                         |
                  | AcademicGateway-Backend |
                  |    (Core API & DB)      |
                  +------------+------------+
                               |
                 +-------------+-------------+
                 |                           |
  Direct DB Sync |                           | Semantic Search
  & Ingestion    v                           v Requests
  +-------------------------------------------------+
  |              AcademicGateway-AI                 |
  |  (FastAPI + PyTorch Embeddings + LanceDB Vector)|
  +-------------------------------------------------+
```

### Key Architectural Highlights
* **Decoupled AI Engine:** Heavy vector calculations and PyTorch embeddings are offloaded to `AcademicGateway-AI` to protect main API latency.
* **CQRS Pattern:** High-volume read semantic searches are decoupled from state-changing ingestion/sync writes.
* **Relational & Vector Hybrid:** Core domain relationships reside in PostgreSQL, while semantic representations live in LanceDB vector storage.

---

## 📁 Repository Structure

```text
AcademicGateway/
├── Tarek_Mourad_Software&AI_AcademicGateway.pdf      # Project report (Named according to code committee specifications)
├── diagrams/                                         # Architectural visual diagrams & schemas
└── README.md                                         # Central ecosystem documentation
```

---
