using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LCcountdownMod2
{
	[HarmonyPatch(typeof(TimeOfDay))]
	internal class TimeOfDayUpdatePatch
	{
		private static bool CountingDown;

		//private static bool CalcuatedWhenToCountdown;

		private static float DownTimer = 3;

		private static int Number;

		private static SelectableLevel PatchcurrentLevel;

		private static InputAction keyAction = new InputAction(null, InputActionType.Button, "<Keyboard>/5");

		private static float DebugTimer;

		//private static float ShipLeavePerdictionNormalized;

		//private static float ShipLeavePerdiction;

		private static float PatchGlobalTime;

		private static float PatchTotalTime;

		private static float EarlyCountDownTime;

		private static float EarlyCountDownDuration = 46.5f;

		private static int PlayersInMatch;

		private static bool DisableDisabler = false;

		private static bool LeavingEarly;
		public static float duration = 3 * 14 + 0.5f;

		[HarmonyPatch("Update")]
		[HarmonyPostfix]
		private static void CountDownPatch(ref float ___currentDayTime, ref SelectableLevel ___currentLevel, ref float ___normalizedTimeOfDay, ref float ___totalTime, ref int ___numberOfHours, ref float ___globalTime, ref int ___votesForShipToLeaveEarly)
		{
			PatchGlobalTime = ___globalTime;
			PatchcurrentLevel = ___currentLevel;
			PatchTotalTime = ___totalTime;
			PlayersInMatch = StartOfRound.Instance.connectedPlayersAmount + 1;
			int num = ((PlayersInMatch <= 1) ? 1 : (StartOfRound.Instance.connectedPlayersAmount + 1 - StartOfRound.Instance.livingPlayers));
			if (LCcountdownMod2.DebugMode)
			{
				if (DebugTimer > 0f)
				{
					DebugTimer -= Time.deltaTime;
				}
				else
				{
					Debug.Log("-GAMETIME-");
					Debug.Log("ClockTime " + GetClock(___normalizedTimeOfDay, ___numberOfHours, createNewLine: false));
					Debug.Log("NormalizedDayTime: " + ___normalizedTimeOfDay);
					Debug.Log("CurrentDayTime: " + ___currentDayTime);
					Debug.Log("NumberOfHours: " + ___numberOfHours);
					Debug.Log("TotalTime " + ___totalTime);
					Debug.Log("StartingCountdown At: " + (LeavingEarly ? GetClock(EarlyCountDownTime / ___totalTime + 0.1f, ___numberOfHours, createNewLine: false) : GetClock(___currentDayTime - duration / ___totalTime, ___numberOfHours, createNewLine: false)));
					Debug.Log("-COUNTDOWN-");
					Debug.Log("EarlyCountDownTime: " + EarlyCountDownTime);
					Debug.Log("LeavingEarly: " + LeavingEarly);
					Debug.Log("CountingDown: " + CountingDown);
					Debug.Log("DownTimer: " + DownTimer);
					Debug.Log("Number: " + Number);
					Debug.Log($"Current EarlyCountDownTime: {EarlyCountDownTime}, CurrentDayTime: {___currentDayTime}, LeavingEarly: {LeavingEarly}");
					DebugTimer = 0.1f;
				}
				keyAction.Enable();
				keyAction.performed += delegate
				{
					DisableDisabler = true;
					DownTimer = 3f;
					Number = 0;
					CountingDown = true;
					LCcountdownMod2.CountDownInstace.Count(LCcountdownMod2.Txts[Number], LCcountdownMod2.TxtSizes[Number]);
					Number++;
					DownTimer = 3f;
				};
			}

			// Replaced the magic numbers with a calculation based on duration
			if (___currentLevel != null)
			{
				float countdownDurationInGameTime = ConvertSecondsToGameTime(duration, ___currentLevel);
				float countdownStartTime = ___totalTime - countdownDurationInGameTime;

				if (___currentDayTime >= countdownStartTime && ___currentDayTime <= countdownStartTime + ConvertSecondsToGameTime(3f, ___currentLevel) && !CountingDown)
				{
					if (___currentDayTime >= 1079f && LCcountdownMod2.StopCountdownAfterTwelve && !DisableDisabler)
					{
						StopCountdown();
						return;
					}
					if (StartOfRound.Instance.shipIsLeaving && LCcountdownMod2.StopCountdownAfterShipLeaves && !DisableDisabler)
					{
						StopCountdown();
						return;
					}
					if (GameNetworkManager.Instance.localPlayerController.isPlayerDead && LCcountdownMod2.StopCountdownAfterDeath)
					{
						StopCountdown();
						return;
					}
					StartCountdown(___currentLevel);
				}
			}


			// Replace the existing if statement starting at line 108 with this:
			if (___currentLevel != null && ___currentDayTime >= EarlyCountDownTime &&
	___currentDayTime <= EarlyCountDownTime + ConvertSecondsToGameTime(3f, ___currentLevel) &&
	LCcountdownMod2.StartCountdownWhenShipLeaveEarly &&
	LeavingEarly &&
	!CountingDown)
			{
				if (___currentDayTime >= 1079f && LCcountdownMod2.StopCountdownAfterTwelve && !DisableDisabler)
				{
					StopCountdown();
					return;
				}
				if (StartOfRound.Instance.shipIsLeaving && LCcountdownMod2.StopCountdownAfterShipLeaves && !DisableDisabler)
				{
					StopCountdown();
					return;
				}
				if (GameNetworkManager.Instance.localPlayerController.isPlayerDead && LCcountdownMod2.StopCountdownAfterDeath)
				{
					StopCountdown();
					return;
				}
				StartCountdown(___currentLevel);
			}

			if (CountingDown)
			{
				if (DownTimer > 0f)
				{
					DownTimer -= Time.deltaTime;
				}
				else if (Number < LCcountdownMod2.Txts.Length)
				{
					LCcountdownMod2.CountDownInstace.Count(LCcountdownMod2.Txts[Number], LCcountdownMod2.TxtSizes[Number]);
					Number++;
					DownTimer = ConvertGameTimeToSeconds(2.6f, ___currentLevel);
				}
				else
				{
					StopCountdown();
				}
				if (___currentDayTime >= 1079f && LCcountdownMod2.StopCountdownAfterTwelve && !DisableDisabler)
				{
					StopCountdown();
				}
				if (StartOfRound.Instance.shipIsLeaving && LCcountdownMod2.StopCountdownAfterShipLeaves && !DisableDisabler)
				{
					StopCountdown();
				}
				if (GameNetworkManager.Instance.localPlayerController.isPlayerDead && LCcountdownMod2.StopCountdownAfterDeath)
				{
					StopCountdown();
				}
				if (StartOfRound.Instance.inShipPhase)
				{
					StopCountdown();
				}
			}
		}

		[HarmonyPatch("SetShipLeaveEarlyClientRpc")]
		[HarmonyPostfix]
		public static void PredectShipCountdown(TimeOfDay __instance, float timeToLeaveEarly, int votes)
		{
			float countdownDurationInGameTime = ConvertSecondsToGameTime(duration, __instance.currentLevel);
			EarlyCountDownTime = (timeToLeaveEarly * __instance.totalTime) - countdownDurationInGameTime;
			LeavingEarly = true;
			Debug.Log($"EarlyCountDownTime calculated: {EarlyCountDownTime}. ShipLeavePerdiction: {timeToLeaveEarly * __instance.totalTime}, duration in game time: {countdownDurationInGameTime}");
		}

		public static float ConvertGameTimeToSeconds(float gameTime, SelectableLevel level)
		{
			return gameTime / level.DaySpeedMultiplier;
		}
		public static float ConvertSecondsToGameTime(float seconds, SelectableLevel level)
		{
			return seconds * level.DaySpeedMultiplier;
		}
		private static void StartCountdown(SelectableLevel currentLevel)
		{
			DisableDisabler = false;
			DownTimer = ConvertGameTimeToSeconds(2.6f, currentLevel);
			Number = 0;
			CountingDown = true;
			LCcountdownMod2.CountDownInstace.Count(LCcountdownMod2.Txts[Number], LCcountdownMod2.TxtSizes[Number]);
			Number++;
		}
		public static void StopCountdown()
		{
			Number = 0;
			DownTimer = 0f;
			CountingDown = false;
			DisableDisabler = false;
			LeavingEarly = false;
		}

		public static string GetClock(float timeNormalized, float numberOfHours, bool createNewLine = true)
		{
			int num = (int)(timeNormalized * (60f * numberOfHours)) + 360;
			int num2 = (int)Mathf.Floor(num / 60);
			string newLine = "";
			string amPM = "";
			newLine = (createNewLine ? "\n" : " ");
			amPM = newLine + "AM";
			if (num2 >= 24)
			{
				return "12:00\nAM";
			}
			amPM = ((num2 >= 12) ? (newLine + "PM") : (newLine + "AM"));
			if (num2 > 12)
			{
				num2 %= 12;
			}
			int num3 = num % 60;
			return $"{num2:00}:{num3:00}".TrimStart(new char[1] { '0' }) + amPM;
		}

		public static float PlanetTime()
		{
			return (PatchGlobalTime + PatchcurrentLevel.OffsetFromGlobalTime) * PatchcurrentLevel.DaySpeedMultiplier % (PatchTotalTime + 1f);
		}
	}
}
