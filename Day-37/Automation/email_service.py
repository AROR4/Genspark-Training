import smtplib
from email.message import EmailMessage

from config import (
    EMAIL_ADDRESS,
    EMAIL_PASSWORD,
    RECEIVER_EMAIL
)


class EmailService:

    def send_email(
        self,
        subject: str,
        body: str
    ):

        try:

            message = EmailMessage()

            message["Subject"] = subject
            message["From"] = EMAIL_ADDRESS
            message["To"] = RECEIVER_EMAIL

            message.add_alternative(
                body,
                subtype="html"
            )
            
            with smtplib.SMTP_SSL(
                "smtp.gmail.com",
                465
            ) as smtp:

                smtp.login(
                    EMAIL_ADDRESS,
                    EMAIL_PASSWORD
                )

                smtp.send_message(
                    message
                )

            print(
                "Email sent successfully."
            )

        except Exception as ex:

            raise Exception(
                f"Email sending failed: {ex}"
            )
    

    @staticmethod
    def build_email(analysis: dict) -> str:

        def build_list(items):
            return "".join(
                f"<li>{item}</li>"
                for item in items
            )
        
        return f"""

        <!DOCTYPE html>

        <html>
        <head>
            <meta charset="UTF-8">
        </head>

        <body style="
            font-family: Arial, sans-serif;
            background-color: #f4f6f9;
            padding: 20px;
        ">

        <div style="
            max-width: 900px;
            margin: auto;
            background: white;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        ">

        <div style="
            background-color: #0f4c81;
            color: white;
            padding: 20px;
        ">
            <h1 style="margin:0;">
                Requirement Analysis Report
            </h1>

            <p style="margin-top:10px;">
                Insurance Claim Processing System
            </p>
        </div>

        <div style="padding:30px;">

            <p>Dear Client,</p>

            <p>
                Thank you for sharing your requirements.
                Our AI-powered Business Analysis Engine has
                completed the initial requirement assessment.
            </p>

            <h2 style="color:#0f4c81;">
                Functional Requirements
            </h2>

            <ul>
                {build_list(analysis["functional_requirements"])}
            </ul>

            <h2 style="color:#0f4c81;">
                Non-Functional Requirements
            </h2>

            <ul>
                {build_list(analysis["non_functional_requirements"])}
            </ul>

            <h2 style="color:#d97706;">
                Risks Identified
            </h2>

            <ul>
                {build_list(analysis["risks"])}
            </ul>

            <h2 style="color:#15803d;">
                Assumptions
            </h2>

            <ul>
                {build_list(analysis["assumptions"])}
            </ul>

            <h2 style="color:#7c3aed;">
                Questions for Clarification
            </h2>

            <ul>
                {build_list(analysis["questions"])}
            </ul>

            <div style="
                margin-top:30px;
                padding:20px;
                background:#f8fafc;
                border-left:5px solid #0f4c81;
            ">
                <strong>Next Steps</strong>

                <p>
                    Please review the identified risks,
                    assumptions, and clarification questions.
                    Once confirmed, the development team can
                    proceed with detailed design and planning.
                </p>
            </div>

            <br>

            <p>
                Regards,
                <br>
                <strong>Business Analysis Team</strong>
                <br>
                Presidio
            </p>

        </div>
        
        </div>

        </body>
        </html>
        """
