import json


class ResponseParser:

    @staticmethod
    def parse(response: str):

        try:

            cleaned_response = response.strip()

            if cleaned_response.startswith("```json"):
                cleaned_response = (
                    cleaned_response
                    .replace("```json", "")
                    .replace("```", "")
                    .strip()
                )

            return json.loads(
                cleaned_response
            )

        except json.JSONDecodeError as ex:

            raise ValueError(
                f"Invalid JSON returned by Gemini: {ex}"
            )