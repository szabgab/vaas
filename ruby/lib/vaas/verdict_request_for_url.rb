module VAAS
    class VerdictRequestForUrl
  
      attr_reader :url, :session_id, :guid
  
      def initialize(url, session_id, guid)
        @kind = "VerdictRequestForUrl"
        @url = url
        @session_id = session_id,
        @guid = guid
      end
  
    end
  end
  