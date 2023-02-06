package messages

import (
	"github.com/google/uuid"
	"vaas/pkg/options"
)

type VerdictRequest struct {
	Kind      string `json:"kind" default:"VerdictRequest"`
	Sha256    string `json:"sha256"`
	Guid      string `json:"guid"`
	SessionID string `json:"session_id"`
	UseCache  bool   `json:"use_cache"`
	UseShed   bool   `json:"use_shed"`
}

func NewVerdictRequest(sessionId string, options options.VaasOptions, sha256 string) VerdictRequest {
	return VerdictRequest{
		Kind:      "VerdictRequest",
		Sha256:    sha256,
		SessionID: sessionId,
		Guid:      uuid.New().String(),
		UseCache:  options.UseCache,
		UseShed:   options.UseShed,
	}
}
