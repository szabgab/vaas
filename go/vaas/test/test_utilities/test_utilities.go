package test_utilities

import (
	"log"
	"os"
	"vaas/pkg/messages"
)

func ReadCredentials() (string, string, string) {
	CLIENT_ID, exists := os.LookupEnv("CLIENT_ID")
	if !exists {
		log.Fatal("no Client ID set")
	}
	CLIENT_SECRET, exists := os.LookupEnv("CLIENT_SECRET")
	if !exists {
		log.Fatal("no Client Secret set")
	}
	TOKEN_ENDPOINT, exists := os.LookupEnv("TOKEN_ENDPOINT")
	if !exists {
		log.Fatal("no token endpoint configured")
	}
	return CLIENT_ID, CLIENT_SECRET, TOKEN_ENDPOINT
}

func Index(s []messages.VaasVerdict, str string) int {
	for i, v := range s {
		if v.Sha256 == str {
			return i
		}
	}

	return -1
}