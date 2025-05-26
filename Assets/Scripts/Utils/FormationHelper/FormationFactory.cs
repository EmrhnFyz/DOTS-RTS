public static class FormationFactory
{
	/// <summary>
	///     Creates the requested FormationBase subclass.
	/// </summary>
	/// <param name="type">Which formation shape to produce</param>
	/// <param name="spacing">Distance between units</param>
	public static T Create<T>(FormationType type, float spacing = 1f) where T : FormationBase
	{
		FormationBase formation = type switch
		{
			FormationType.Square => new SquareFormation(spacing),
			FormationType.Circle => new CircleFormation(spacing),
			FormationType.ArrowOutline => new OutlineArrowFormation(spacing),
			FormationType.ArrowFilled => new FilledArrowFormation(spacing),
			_ => new SquareFormation(spacing)
		};

		return formation as T;
	}
}