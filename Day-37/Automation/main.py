from prompt_builder import PromptBuilder
from gemini_service import GeminiService
from email_service import EmailService
from parser import ResponseParser
import json
from pathlib import Path

try:

    prompt_builder = PromptBuilder(
        prompt_file_path="prompts/requirement_analysis.prompt",
        requirement_file_path="requirements/client_requirement.docx"
    )

    final_prompt = prompt_builder.build_prompt()

    print("Prompt generated successfully.")

    gemini_service = GeminiService()

    response = gemini_service.analyze_requirement(
        final_prompt
    )

    print("Requirement analysis completed successfully.")

    analysis = ResponseParser.parse(response)

    Path("outputs/analysis.json").write_text(
        json.dumps(
            analysis,
            indent=4
        ),
        encoding="utf-8"
    )

    print("Analysis saved successfully.")

    Path("outputs/gemini_conversation.txt").write_text(
        f"""
        PROMPT
        {'=' * 50}

        {final_prompt}

        RESPONSE
        {'=' * 50}

        {response}
        """,
            encoding="utf-8"
        )

    print("Conversation log saved successfully.")

    email_service = EmailService()
    email_service.send_email(
        subject=f"Requirement Analysis Report for {analysis['project_name']}",
        body=email_service.build_email(analysis))


except Exception as ex:

    print(f"Application Error: {ex}")