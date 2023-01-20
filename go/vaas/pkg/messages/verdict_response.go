package messages

import (
	"net/url"
)

type VerdictResponse struct {
	Kind        string  `json:"kind"`
	Sha256      string  `json:"sha256"`
	Guid        string  `json:"guid"`
	Verdict     Verdict `json:"verdict"`
	Url         url.URL `json:"url"`
	UploadToken string  `json:"upload_token"`
}

func (response VerdictResponse) IsValid() bool {
	return response.Sha256 != "" && response.Verdict != ""
}
