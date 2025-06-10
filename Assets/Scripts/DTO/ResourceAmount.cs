using System;

[Serializable]
public struct ResourceAmount
{
	public ResourceType ResourceType;
	public int Amount;

	public static string GetString(ResourceAmount[] resourceAmountArray)
	{
		var resourceAmountString = "";
		foreach (var resourceAmount in resourceAmountArray)
		{
			if (resourceAmountString != null)
			{
				resourceAmountString += "\n";
			}

			resourceAmountString += resourceAmount.ResourceType + " x" + resourceAmount.Amount;
		}

		return resourceAmountString;
	}
}