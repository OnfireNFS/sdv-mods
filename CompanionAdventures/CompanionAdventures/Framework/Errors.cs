namespace CompanionAdventures.Framework;

public class PropertyNullException(string property) : Exception($"Property: {property} was accessed before being initialized!");