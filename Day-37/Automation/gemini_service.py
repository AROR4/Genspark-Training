import google.generativeai as genai

from config import GEMINI_API_KEY


class GeminiService:

    def __init__(self):

        if not GEMINI_API_KEY:
            raise ValueError(
                "GEMINI_API_KEY is missing in .env file."
            )

        genai.configure(
            api_key=GEMINI_API_KEY
        )

        self.model = genai.GenerativeModel(
            "gemini-2.5-flash"
        )

    def analyze_requirement(
        self,
        prompt: str
    ) -> str:

        try:

            response = self.model.generate_content(
                prompt
            )

            if not response.text:
                raise ValueError(
                    "Gemini returned an empty response."
                )

            return response.text

        except Exception as ex:

            raise Exception(
                f"Gemini API error: {ex}"
            )