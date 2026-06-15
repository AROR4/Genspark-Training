# AI-Powered Requirement Analysis & Email Automation

## Overview

This project automates the requirement analysis process by leveraging Generative AI to extract key business insights from client requirement documents and automatically generate a professional email report.

The system reads a client requirement document, applies a structured business analysis prompt, generates a detailed requirement assessment using Gemini AI, formats the results into a professional HTML email, and delivers the report directly to a recipient's inbox.

---

## Objectives

* Automate requirement gathering analysis
* Extract business insights from requirement documents
* Generate structured requirement reports
* Identify ambiguities and risks early in the project lifecycle
* Automatically communicate findings through email

---

## Features

### Requirement Analysis

The AI engine automatically identifies:

* Functional Requirements
* Non-Functional Requirements
* Risks
* Assumptions
* Questions to Client

### Prompt Engineering

The solution utilizes:

* Role-Based Prompting
* Few-Shot Prompting
* Structured JSON Output
* Domain-Specific Context Injection
* Guardrail-Based Response Formatting

### Email Automation

* Generates professional HTML email reports
* Sends reports using Gmail SMTP

### Error Handling

* Missing requirement file validation
* Empty document validation
* Missing prompt validation
* Gemini API exception handling
* JSON parsing validation
* Email delivery exception handling

---

## Architecture

```text

.env
    +
Requirement Document (.docx)
    +
Prompt Template

        ↓

   Prompt Builder

        ↓

     Gemini AI

        ↓

 Requirement Analysis

        ↓

   analysis.json

        ↓

 HTML Email Generator

        ↓

    Gmail SMTP

        ↓

  Recipient Inbox
```

---

## Project Structure

```text
Automation/

├── docs/
│
├── outputs/
│   ├── analysis.json
│   └── gemini_conversation.txt
│
├── prompts/
│   └── requirement_analysis.prompt
│
├── requirements/
│   └── client_requirement.docx
│
├── config.py
├── prompt_builder.py
├── gemini_service.py
├── parser.py
├── email_service.py
├── main.py
│
├── .env
├── pyproject.toml
├── uv.lock
└── README.md
```

---

## Technology Stack

| Component             | Technology       |
| --------------------- | ---------------- |
| Programming Language  | Python           |
| AI Model              | Gemini 2.5 Flash |
| Package Management    | UV               |
| Document Processing   | python-docx      |
| Environment Variables | python-dotenv    |
| Email Delivery        | Gmail SMTP       |
| Data Format           | JSON             |

---

## Workflow

### Step 1

Read client requirement document.

```text
client_requirement.docx
```

### Step 2

Load few-shot business analysis prompt.

```text
requirement_analysis.prompt
```

### Step 3

Inject requirement content into prompt template.

### Step 4

Submit prompt to Gemini AI.

### Step 5

Generate structured requirement analysis.

```json
{
  "functional_requirements": [],
  "non_functional_requirements": [],
  "risks": [],
  "assumptions": [],
  "questions": []
}
```

### Step 6

Save analysis results.

```text
outputs/analysis.json
```

### Step 7

Generate professional HTML email.

### Step 8

Send email using Gmail SMTP.

---

## Prompt Engineering Techniques

### Role Prompting

```text
You are a Senior Product Manager specializing in Insurance Systems.
```

### Few-Shot Prompting

Example input-output pairs are included to guide the model toward consistent and structured responses.

### Structured Output Prompting

The model is constrained to return valid JSON.

### Domain Context Injection

Insurance-specific business analysis context improves output relevance.

---

## Sample Output

### Functional Requirements

* Manage insurance policies
* Submit insurance claims
* Verify claim documents
* Support approval workflows

### Risks

* Regulatory non-compliance
* Data security breaches
* Incorrect claim processing

### Questions to Client

* What approval workflow is required?
* What notification channels should be supported?
* What integrations are expected?

---

## Deliverables Generated

### Analysis Report

```text
outputs/analysis.json
```

### Conversation Log

```text
outputs/gemini_conversation.txt
```

### Email Report

```text
outputs/generated_email.html
```

---

## Installation

### Clone Repository

```bash
git clone <repository-url>
cd Automation
```

### Create Environment

```bash
uv sync
```

### Install Dependencies

```bash
uv add google-generativeai
uv add python-dotenv
uv add python-docx
```

### Configure Environment Variables

Create `.env`

```env
GEMINI_API_KEY=YOUR_API_KEY

EMAIL_ADDRESS=your_email@gmail.com

EMAIL_PASSWORD=your_app_password

RECEIVER_EMAIL=recipient@gmail.com
```

### Run Application

```bash
uv run main.py
```



