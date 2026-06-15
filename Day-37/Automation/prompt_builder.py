from pathlib import Path
from docx import Document


class PromptBuilder:
    def __init__(
        self,
        prompt_file_path: str,
        requirement_file_path: str
    ):
        self.prompt_file_path = prompt_file_path
        self.requirement_file_path = requirement_file_path

    def load_prompt(self) -> str:
        try:
            return Path(
                self.prompt_file_path
            ).read_text(
                encoding="utf-8"
            )

        except FileNotFoundError:
            raise FileNotFoundError(
                f"Prompt file not found: {self.prompt_file_path}"
            )

        except Exception as ex:
            raise Exception(
                f"Failed to load prompt file: {ex}"
            )

    def load_requirement(self) -> str:

        try:

            file_path = Path(
                self.requirement_file_path
            )

            if file_path.suffix == ".txt":

                return file_path.read_text(
                    encoding="utf-8"
                )
            
            if file_path.suffix == ".docx":

                document = Document(
                    self.requirement_file_path
                )

                return "\n".join(
                    paragraph.text
                    for paragraph in document.paragraphs
                )

            return file_path.read_text(
                encoding="utf-8"
            )

        except FileNotFoundError:

            raise FileNotFoundError(
                f"Requirement file not found: {self.requirement_file_path}"
            )

        except Exception as ex:

            raise Exception(
                f"Failed to load requirement file: {ex}"
            )

    def inject_requirement(
        self,
        prompt_template: str,
        requirement: str
    ) -> str:

        if not prompt_template.strip():
            raise ValueError(
                "Prompt template is empty."
            )

        if not requirement.strip():
            raise ValueError(
                "Requirement document is empty."
            )

        if "{{requirement}}" not in prompt_template:
            raise ValueError(
                "Placeholder {{requirement}} not found in prompt template."
            )

        return prompt_template.replace(
            "{{requirement}}",
            requirement
        )

    def build_prompt(self) -> str:
        try:
            prompt_template = self.load_prompt()

            requirement = self.load_requirement()

            final_prompt = self.inject_requirement(
                prompt_template,
                requirement
            )

            return final_prompt

        except Exception as ex:
            raise Exception(
                f"Prompt generation failed: {ex}"
            )