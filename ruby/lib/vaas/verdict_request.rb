module VAAS
    class VerdictRequest
  
      attr_reader :sha256, :session_id, :guid
  
      def initialize(sha256, session_id, guid)
        @kind = "VerdictRequest"
        @sha256 = sha256
        @session_id = session_id,
        @guid = guid
      end
  
    end
  end
  