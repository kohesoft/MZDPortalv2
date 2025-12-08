using System;
using System.Collections.Generic;
using System.Linq;

namespace MZDNETWORK.Helpers
{
    /// <summary>
    /// Translates legacy (English) permission path segments into Turkish, ASCII-safe equivalents.
    /// Keeps original as an alias so old data keeps working during migration.
    /// </summary>
    public static class PermissionPathTranslator
    {
        private static readonly Dictionary<string, string> RootMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "KullaniciYonetimi", "KullaniciYonetimi" },
            { "RolYonetimi", "RolYonetimi" },
            { "SistemYonetimi", "SistemYonetimi" },
            { "Operasyon", "Operasyon" },
            { "InsanKaynaklari", "InsanKaynaklari" },
            { "BilgiIslem", "BilgiIslem" },
            { "Yemek", "Yemek" }
        };

        private static readonly Dictionary<string, string> SegmentMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "AccessChange", "YetkiDegisikligi" },
            { "PasswordResetRequests", "SifreSifirlamaTalepleri" },
            { "UserInfo", "KullaniciBilgisi" },
            { "KullaniciYonetimi", "KullaniciIslemleri" },
            { "RolePermission", "RolYetkileri" },
            { "PermissionTree", "YetkiAgaci" },
            { "RolYonetimi", "RolIslemleri" },
            { "Notification", "Bildirim" },
            { "Performance", "Performans" },
            { "Chat", "Sohbet" },
            { "Contact", "Iletisim" },
            { "DailyMood", "GunlukMood" },
            { "LeaveRequest", "IzinTalebi" },
            { "MeetingRoom", "ToplantiOdasi" },
            { "Suggestion", "Oneri" },
            { "Reply", "Cevap" },
            { "Survey", "Anket" },
            { "Task", "Gorev" },
            { "WhiteBoard", "BeyazTahta" },
            { "Entry", "Girdi" },
            { "UpdateHeader", "BaslikGuncelle" },
            { "VisitorEntry", "ZiyaretciGiris" },
            { "LateArrivalReport", "GecGelisRaporu" },
            { "Announcements", "Duyurular" },
            { "YemekPhoto", "YemekFoto" },
            { "BreakPhoto", "MolaFoto" },
            { "Merkez", "Merkez" },
            { "Yerleske", "Yerleske" }
        };

        /// <summary>
        /// Returns the canonical (translated) path. If no translation applies, returns the original.
        /// </summary>
        public static string ToCanonical(string permissionPath)
        {
            if (string.IsNullOrWhiteSpace(permissionPath))
                return permissionPath;

            var parts = permissionPath.Split('.');
            if (parts.Length == 0)
                return permissionPath;

            var translated = new string[parts.Length];

            // Root segment
            var root = parts[0];
            translated[0] = RootMap.TryGetValue(root, out var rootTranslated) ? rootTranslated : root;

            for (int i = 1; i < parts.Length; i++)
            {
                var segment = parts[i];
                translated[i] = SegmentMap.TryGetValue(segment, out var mapped) ? mapped : segment;
            }

            return string.Join(".", translated);
        }

        /// <summary>
        /// Returns canonical + original variants (distinct, canonical first) so callers can stay compatible during migration.
        /// </summary>
        public static IEnumerable<string> GetPathVariants(string permissionPath)
        {
            if (string.IsNullOrWhiteSpace(permissionPath))
                yield break;

            var canonical = ToCanonical(permissionPath);
            yield return canonical;

            if (!canonical.Equals(permissionPath, StringComparison.OrdinalIgnoreCase))
            {
                yield return permissionPath;
            }
        }
    }
}

