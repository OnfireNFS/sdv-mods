namespace CompanionFramework.Framework;

public class PropertyNullException(string property) : Exception($"Property: \"{property}\" was accessed before being initialized!");

public class CompanionException(string msg) : Exception(msg);
public class CompanionNotFoundException(string name) : CompanionException($"Could not find companion with name: \"{name}\"!");
public class CompanionAlreadyRecruitedException(string name) : CompanionException($"Could not recruit companion \"{name}\". \"{name}\" is already recruited!");
public class CompanionNotRecruitedException(string name) : CompanionException($"Companion \"{name}\" is not recruited!");
public class CompanionNotFollowingFarmerException(string name, string farmer) : CompanionException($"Companion \"{name}\" is not following farmer \"{farmer}\"!"); 