using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rise.Client.Profile.Models;

namespace Rise.Client.Profile.Components;

public partial class ProfileScreen
{
    private List<string> OrderPreferenceIds(IEnumerable<string> ids)
    {
        return ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => _preferenceOrderById.TryGetValue(id, out var order) ? order : int.MaxValue)
            .ThenBy(id => GetPreferenceName(id), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private string ResolvePreferenceId(string value)
    {
        var normalized = NormalizePreferenceValue(value);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        if (_preferenceOptionsById.TryGetValue(normalized, out var optionById))
        {
            return optionById.Id;
        }

        if (_preferenceIdByName.TryGetValue(normalized, out var optionId))
        {
            return optionId;
        }

        if (!_customPreferenceOptions.ContainsKey(normalized))
        {
            _customPreferenceOptions[normalized] = normalized;
        }

        return normalized;
    }

    private static string NormalizePreferenceValue(string value)
        => value.Trim();

    private string GetPreferenceName(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return string.Empty;
        }

        if (_preferenceOptionsById.TryGetValue(id, out var option))
        {
            return option.Name;
        }

        if (_customPreferenceOptions.TryGetValue(id, out var custom))
        {
            return custom;
        }

        return id;
    }

    private string GetPreferenceLabel(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return string.Empty;
        }

        if (_preferenceOptionsById.TryGetValue(id, out var option))
        {
            return option.Label;
        }

        if (_customPreferenceOptions.TryGetValue(id, out var custom))
        {
            return custom;
        }

        return id;
    }

    private static string FormatBirthDay(DateOnly birthDay)
    {
        if (birthDay == default)
        {
            return "–";
        }

        return birthDay.ToString("dd MMMM yyyy", CultureInfo.CurrentCulture);
    }

    private IReadOnlyList<PreferenceChip> BuildPreferenceChips(IEnumerable<string> ids)
    {
        var chips = new List<PreferenceChip>();
        foreach (var id in ids)
        {
            var label = GetPreferenceLabel(id);
            if (!string.IsNullOrWhiteSpace(label))
            {
                chips.Add(new PreferenceChip(id, label));
            }
        }

        return chips;
    }

    private List<string> OrderChatLineIds(IEnumerable<string> ids)
    {
        return ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => _chatLineOrderById.TryGetValue(id, out var order) ? order : int.MaxValue)
            .ThenBy(id => GetChatLineText(id), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private string ResolveChatLineId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim();

        if (_chatLineOptionsById.TryGetValue(normalized, out var optionById))
        {
            return optionById.Id;
        }

        if (_chatLineIdByName.TryGetValue(normalized, out var optionId))
        {
            return optionId;
        }

        if (!_customChatLineOptions.ContainsKey(normalized))
        {
            _customChatLineOptions[normalized] = normalized;
        }

        return normalized;
    }

    private string GetChatLineText(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return string.Empty;
        }

        if (_chatLineOptionsById.TryGetValue(id, out var option))
        {
            return option.Name;
        }

        if (_customChatLineOptions.TryGetValue(id, out var custom))
        {
            return custom;
        }

        if (_chatLineIdByName.TryGetValue(id, out var resolvedId) && _chatLineOptionsById.TryGetValue(resolvedId, out var resolvedOption))
        {
            return resolvedOption.Name;
        }

        return id;
    }

    private IReadOnlyList<PreferenceChip> BuildChatLineChips(IEnumerable<string> ids)
    {
        var chips = new List<PreferenceChip>();
        foreach (var id in ids)
        {
            var text = GetChatLineText(id);
            if (!string.IsNullOrWhiteSpace(text))
            {
                chips.Add(new PreferenceChip(id, text));
            }
        }

        return chips;
    }

    private IReadOnlyList<string> BuildChatLineTexts(IEnumerable<string> ids)
    {
        var result = new List<string>();
        foreach (var id in ids)
        {
            var text = GetChatLineText(id);
            if (!string.IsNullOrWhiteSpace(text))
            {
                result.Add(text);
            }
        }

        return result;
    }

    private static bool RemovePreference(List<string> list, string id)
    {
        var index = list.FindIndex(existing => string.Equals(existing, id, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            list.RemoveAt(index);
            return true;
        }

        return false;
    }

    private void UpdateInterestsModel()
    {
        var interests = new List<ProfileInterestModel>();

        foreach (var likeId in _selectedLikeIds)
        {
            var label = GetPreferenceName(likeId);
            if (!string.IsNullOrWhiteSpace(label))
            {
                interests.Add(new ProfileInterestModel("Like", label, null));
            }
        }

        foreach (var dislikeId in _selectedDislikeIds)
        {
            var label = GetPreferenceName(dislikeId);
            if (!string.IsNullOrWhiteSpace(label))
            {
                interests.Add(new ProfileInterestModel("Dislike", null, label));
            }
        }

        _model = _model with { Interests = interests };
    }

    private static ProfileHobbyModel? CreateHobbyModel(string id)
    {
        var option = _hobbyOptions.FirstOrDefault(o => string.Equals(o.Id, id, StringComparison.Ordinal));
        return option is null ? null : new ProfileHobbyModel(option.Id, option.Name, option.Emoji);
    }
}
