namespace InteractiveSdk.WorkerRole
{
	internal interface IMessageHandler
	{
		/// <summary>
		/// Returns <c>true</c> if message was processed by this handler
		/// </summary>
		/// <param name="msg">Message body.</param>
		void ProcessMessage(string msg);
	}
}
