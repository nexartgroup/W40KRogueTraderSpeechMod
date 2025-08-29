using Kingmaker;
using Kingmaker.Blueprints.Base;
using SpeechMod.Unity;
using System;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SpeechMod.Voice;

public class AppleSpeech : ISpeech
{
    private static string SpeakBegin => "";
    private static string SpeakEnd => "";

    private static string NarratorVoice => $"<voice required=\"Name={Main.NarratorVoice}\">";
    private static string NarratorPitch => $"<pitch absmiddle=\"{Main.Settings?.NarratorPitch}\"/>";
    private static string NarratorRate => $"<rate absspeed=\"{Main.Settings?.NarratorRate}\"/>";
    private static string NarratorVolume => $"<volume level=\"{Main.Settings?.NarratorVolume}\"/>";

    private static string FemaleVoice => $"<voice required=\"Name={Main.FemaleVoice}\">";
    private static string FemaleVolume => $"<volume level=\"{Main.Settings?.FemaleVolume}\"/>";
    private static string FemalePitch => $"<pitch absmiddle=\"{Main.Settings?.FemalePitch}\"/>";
    private static string FemaleRate => $"<rate absspeed=\"{Main.Settings?.FemaleRate}\"/>";

    private static string MaleVoice => $"<voice required=\"Name={Main.MaleVoice}\">";
    private static string MaleVolume => $"<volume level=\"{Main.Settings?.MaleVolume}\"/>";
    private static string MalePitch => $"<pitch absmiddle=\"{Main.Settings?.MalePitch}\"/>";
    private static string MaleRate => $"<rate absspeed=\"{Main.Settings?.MaleRate}\"/>";

    public string CombinedNarratorVoiceStart => $"{NarratorVoice}";
    public string CombinedFemaleVoiceStart => $"{FemaleVoice}";
    public string CombinedMaleVoiceStart => $"{MaleVoice}";

    public virtual string CombinedDialogVoiceStart
    {
        get
        {
            if (Game.Instance?.DialogController?.CurrentSpeaker == null)
                return CombinedNarratorVoiceStart;

            return Game.Instance.DialogController.CurrentSpeaker.Gender switch
            {
                Gender.Female => CombinedFemaleVoiceStart,
                Gender.Male => CombinedMaleVoiceStart,
                _ => CombinedNarratorVoiceStart
            };
        }
    }

    public static int Length(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        var arr = new[] { "—", "-", "\"" };

        return arr.Aggregate(text, (current, t) => current.Replace(t, "")).Length;
    }

    private string FormatGenderSpecificVoices(string text)
    {
        text = text.Replace($"<i><color=#{Constants.NARRATOR_COLOR_CODE}>", "");
        text = text.Replace("</color></i>", "");
        return text;
    }

    private void SpeakInternal(string text, float delay = 0f)
    {
        text = SpeakBegin + text + SpeakEnd;
        if (Main.Settings?.LogVoicedLines == true)
            UnityEngine.Debug.Log(text);
        AppleVoiceUnity.Speak(text, delay);
    }

    public bool IsSpeaking()
    {
        return false;
    }

    public void SpeakPreview(string text, VoiceType voiceType)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to speak!");
            return;
        }

        text = text.PrepareText();
        text = new Regex("<[^>]+>").Replace(text, "");

        SpeakAs(text, voiceType);
    }

    public string PrepareSpeechText(string text)
    {
        text = new Regex("<[^>]+>").Replace(text, "");
        text = text.PrepareText();
        return text;
    }

    public string PrepareDialogText(string text)
    {
        text = text.PrepareText();
        text = new Regex("<b><color[^>]+><link([^>]+)?>([^<>]*)</link></color></b>").Replace(text, "$2");
		// text = FormatGenderSpecificVoices(text);
        return text;
    }

    public void SpeakDialog(string text, float delay = 0f)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to speak!");
            return;
        }

        if (!Main.Settings.UseGenderSpecificVoices)
        {
            Speak(text, delay);
            return;
        }

        text = PrepareDialogText(text);

        SpeakInternal(text, delay);
    }

    public void SpeakAs(string text, VoiceType voiceType, float delay = 0f)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			Main.Logger?.Warning("No text to speak!");
			return;
		}

		if (!Main.Settings!.UseGenderSpecificVoices)
		{
			Speak(text, delay);
			return;
		}

		// strip embedded quotes from the spoken text to avoid breaking the format
		var sanitized = text.Replace("\"", "");

		// Build "voice" "text"
		string formatted;
		switch (voiceType)
		{
			case VoiceType.Narrator:
				formatted = $"\"{Main.NarratorVoice}\" \"{sanitized}\"";
				break;

			case VoiceType.Female:
				formatted = $"\"{Main.FemaleVoice}\" \"{sanitized}\"";
				break;

			case VoiceType.Male:
				formatted = $"\"{Main.MaleVoice}\" \"{sanitized}\"";
				break;

			default:
				throw new ArgumentOutOfRangeException(nameof(voiceType), voiceType, null);
		}
		Process.Start(Unity.AppleVoiceUnity.GetScriptPath(), formatted);
	}
    public void Speak(string text, float delay = 0f)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to speak!");
            return;
        }

        text = PrepareSpeechText(text);

        SpeakInternal(text, delay);
    }

    public void Stop()
    {
        AppleVoiceUnity.Stop();
    }

    public string[] GetAvailableVoices()
    {
        return AppleVoiceUnity.GetAvailableVoices();
    }

    public string GetStatusMessage()
    {
        return AppleVoiceUnity.GetStatusMessage();
    }
}