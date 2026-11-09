# Unity Personal license and GitHub Actions setup

This project uses the GameCI v4 Unity Android workflow. The Unity Editor and Android player are built on GitHub-hosted runners; a local Unity Editor installation is not required for routine building.

## 1. Create a Unity account

1. Open https://id.unity.com.
2. Choose **Create account** and register using an email address you can access.
3. Verify the email address and keep the account password private.
4. Use this same Unity ID for Unity Hub and the GitHub Actions secrets.

Unity Personal is free for eligible individuals/hobbyists and organisations whose relevant Total Finances do not exceed USD 200,000 in the most recent twelve-month period. Check the current eligibility terms before using the plan:
- https://unity.com/products/pricing-updates
- https://unity.com/pages/license-compliance

## 2. Activate Unity Personal and obtain the .ulf file

1. On a Windows 10/11, macOS or Linux computer, download Unity Hub from https://unity.com/download.
2. Install and open Unity Hub, then sign in to the Unity account just created.
3. Open **Settings / Preferences → Licenses**.
4. Click **Add** (or **Add license**).
5. Choose **Get a free Personal license** and accept the Personal terms.
6. Important for GameCI: complete the explicit Add activation flow even when the Hub already displays a Personal entitlement. GameCI's current v4 guide warns that an entitlement can appear in the Hub without the .ulf file needed by the runner.
7. On Windows, find the file at:
   **C:\ProgramData\Unity\Unity_lic.ulf**
   An easy way to open the folder is to paste **%PROGRAMDATA%\Unity** into File Explorer's address bar. The ProgramData folder may be hidden.
   On macOS: **/Library/Application Support/Unity/Unity_lic.ulf**.
   On Linux: **~/.local/share/unity3d/Unity/Unity_lic.ulf**.

Only Unity Hub is needed for this activation step. Do not install the full Unity Editor or Android Build Support locally for the GitHub cloud build. GameCI v4 documents that a licence activated on one operating system can be used for cloud builds on another operating system.

If the .ulf file does not exist, return to Hub → Licenses → Add and finish the explicit Personal activation flow. Do not substitute the .xml entitlement file for the .ulf file.

## 3. Add the three GitHub Actions repository secrets

Open the repository:
https://github.com/CybersecurityRahbar/Tactical-Strike-Android

Then open:
https://github.com/CybersecurityRahbar/Tactical-Strike-Android/settings/secrets/actions

For each secret, click **New repository secret**, enter its exact name and value, then **Add secret**:

| Secret name | Value |
| --- | --- |
| UNITY_LICENSE | Open Unity_lic.ulf in a text editor, copy its entire contents and paste them. Do not upload the file to the repository. |
| UNITY_EMAIL | The exact email address used to sign in to Unity. |
| UNITY_PASSWORD | The password for that same Unity account. |

Create all three names exactly as shown, with no spaces before or after. Do not send any of these values in a chat, commit them to Git, add them to README/context files, or place them in workflow YAML. GitHub encrypts repository secrets and makes them available to Actions only when a workflow explicitly requests them.

The current project workflow checks that all three secrets are present before starting the asset/build jobs.

## 4. Start the Unity Android build

1. Open https://github.com/CybersecurityRahbar/Tactical-Strike-Android/actions.
2. Select **Unity Android Build** in the left workflow list.
3. Click **Run workflow**, leave the branch set to **main**, and confirm **Run workflow**.
4. Open the new run to inspect its jobs and logs.
5. When the run succeeds, open its **Artifacts** section and download **TacticalStrike-Unity-Android-verified**.
6. Extract the downloaded outer ZIP once. Inside is TacticalStrike-debug.apk and validation/hash reports. Install the APK directly; do not extract the APK itself.

A failed run is not an APK. Open the failed step and use its error output to identify whether the issue is missing secrets, Unity licence reactivation, asset preparation, Unity import/C# compilation, or Android packaging.

## Security and troubleshooting

- Never give the Unity password or .ulf licence contents to another person or paste them into a chat.
- If the Unity account has two-factor authentication, complete the Hub browser/device sign-in prompts. Keep the recovery codes private.
- If the Unity password contains special characters and GameCI's login step reports an authentication/parsing issue, inspect the exact error first. Do not weaken a good password preemptively; change it only through Unity's official account settings and keep a strong, unique password.
- If Unity Hub displays a Personal entitlement but no .ulf file exists, repeat **Licenses → Add → Get a free Personal license** as described above.
- For updated GameCI v4 activation/build instructions, see https://game.ci/docs/github/activation/ and https://game.ci/docs/github/builder/.
- For GitHub's official repository-secret procedure, see https://docs.github.com/en/actions/how-tos/write-workflows/choose-what-workflows-do/use-secrets.

## Status at the time this document was added

The Unity project and manual workflow are already in the repository. The Unity account and three secrets have **not** been supplied or verified by the assistant, and the Unity Android workflow has **not** yet been run with a valid Personal licence. After the repository owner completes the steps above, the Unity workflow can be started.
