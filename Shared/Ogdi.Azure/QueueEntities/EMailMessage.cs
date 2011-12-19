namespace Ogdi.Azure.QueueEntities
{
	/// <summary>
	/// Represents a serializable message.
	/// </summary>
	public sealed class EMailMessage
	{
		public string To;
		public string Subject;
		public string Body;
		public bool IsBodyHtml;
	}
}
