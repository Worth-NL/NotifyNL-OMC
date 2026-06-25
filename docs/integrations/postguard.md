# PostGuard

PostGuard integration was introduced in OMC **v2.0.0**. It enables delivery of encrypted PDF documents to citizens via their [Yivi](https://yivi.app/) digital identity wallet, providing a secure channel for sensitive correspondence.

---

## How it works

When a decision or document needs to be delivered securely as an encrypted PDF:

1. OMC uploads the PDF package to PostGuard via the package upload endpoint
2. PostGuard encrypts the PDF using the Cryptify endpoint, binding it to the citizen's Yivi identity
3. OMC sends a notification to the citizen via NotifyNL (email or SMS) informing them that a secure document is waiting
4. The citizen opens their Yivi wallet to decrypt and read the document

---

## Configuration

| Variable | Description |
|---|---|
| `POSTGUARD_API_KEY` | API key for the PostGuard service |
| `POSTGUARD_API_PKGURL` | PostGuard package upload endpoint URL |
| `POSTGUARD_API_CRYPTIFYURL` | PostGuard encryption (Cryptify) endpoint URL |
| `POSTGUARD_TEMPLATEID_SENDPOSTGUARDPDF` | NotifyNL template ID for the notification sent to citizens about the encrypted document |

---

## NotifyNL template

The `POSTGUARD_TEMPLATEID_SENDPOSTGUARDPDF` template is a standard NotifyNL email or SMS template that informs the citizen that a secure document is available in their Yivi wallet. The template should not include the document content itself — only a reference and instructions to open Yivi.

> Contact [Worth Systems](mailto:info@worth.nl) for guidance on PostGuard template setup and Cryptify endpoint configuration, as this depends on your PostGuard account and Yivi configuration.

---

## Requirements

- An active PostGuard account with access to the package upload and Cryptify endpoints
- Citizens must have the Yivi app installed and have verified their identity
- This feature is only available in workflow v2 and above
