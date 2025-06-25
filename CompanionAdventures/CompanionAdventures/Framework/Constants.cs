using StardewModdingAPI;

namespace CompanionAdventures.Framework;

public static class Constants
{
    public static readonly ISemanticVersion MinHostVersion = new SemanticVersion(1, 0, 0);
    public const string DialogApprove = "CompanionAdventures.Dialog.Approve";
    public const string DialogReject = "CompanionAdventures.Dialog.Reject";
}