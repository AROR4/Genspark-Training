using NotificationModelLibrary.Enums;
using NotificationModelLibrary.Exceptions;
using System.Text.RegularExpressions;

namespace NotificationBLLibrary.Validators
{
    public static class NotificationValidator
    {
        public static void ValidateMessage(string message, NotificationType notificationType)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new InvalidMessageException("Message cannot be empty.");
            }
            if (message.Length < 5)
            {
                throw new InvalidMessageException("Message length should be at least 5 characters.");
            }
            if (notificationType == NotificationType.Sms && message.Length > 160)
            {
                throw new InvalidMessageException("Sms message length should not exceed 160 characters.");
            }
        }
    }
}