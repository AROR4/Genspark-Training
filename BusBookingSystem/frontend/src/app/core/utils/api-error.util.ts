export function formatApiError(error: unknown, fallback = 'Something went wrong.'): string {
  const response = error as {
    error?: {
      title?: string;
      message?: string;
      errors?: Record<string, string[]>;
    };
    message?: string;
  };

  const validationErrors = response?.error?.errors;
  if (validationErrors && typeof validationErrors === 'object') {
    const messages = Object.entries(validationErrors)
      .flatMap(([field, errors]) => (errors || []).map(message => `${humanizeField(field)}: ${message}`));

    if (messages.length) {
      return messages.join(' ');
    }
  }

  if (response?.error?.message) {
    return response.error.message;
  }

  if (response?.error?.title) {
    return response.error.title;
  }

  if (response?.message) {
    return response.message;
  }

  return fallback;
}

function humanizeField(field: string): string {
  return field
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .replace(/_/g, ' ')
    .replace(/\b\w/g, match => match.toUpperCase());
}
