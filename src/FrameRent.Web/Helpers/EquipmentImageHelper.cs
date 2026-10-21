namespace FrameRent.Web.Helpers;

public static class EquipmentImageHelper
{
    public static string GetImageUrl(string category, string name)
    {
        var cat = (category ?? string.Empty).ToLowerInvariant();
        var n = (name ?? string.Empty).ToLowerInvariant();

        if (n.Contains("fx3") || n.Contains("cinema") || cat.Contains("camera"))
        {
            return "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=800&q=80"; // Pro Cinema Camera
        }

        if (n.Contains("lens") || n.Contains("canon rf") || cat.Contains("lens"))
        {
            return "https://images.unsplash.com/photo-1617005082133-548c4dd27f35?auto=format&fit=crop&w=800&q=80"; // Camera Lens
        }

        if (n.Contains("light") || n.Contains("aputure") || cat.Contains("light"))
        {
            return "https://images.unsplash.com/photo-1574717024653-61fd2cf4d44d?auto=format&fit=crop&w=800&q=80"; // Studio Lighting
        }

        if (n.Contains("mic") || n.Contains("sennheiser") || cat.Contains("audio"))
        {
            return "https://images.unsplash.com/photo-1590602847861-f357a9332bbc?auto=format&fit=crop&w=800&q=80"; // Microphone Studio
        }

        if (n.Contains("gimbal") || n.Contains("dji") || cat.Contains("stabiliz"))
        {
            return "https://images.unsplash.com/photo-1527011046414-4781f1f94f8c?auto=format&fit=crop&w=800&q=80"; // Gimbal
        }

        if (n.Contains("tripod") || n.Contains("sachtler") || cat.Contains("tripod"))
        {
            return "https://images.unsplash.com/photo-1502920917128-1aa500764cbd?auto=format&fit=crop&w=800&q=80"; // Tripod
        }

        // Generic high-end gear fallback
        return "https://images.unsplash.com/photo-1492691527719-9d1e07e534b4?auto=format&fit=crop&w=800&q=80";
    }
}
