import { JsonProperty, JsonObject } from "typescript-json-serializer";
import { Kind, Message } from "./message";

@JsonObject()
export class VerdictRequestForUrl extends Message {
  public constructor(url: URL, guid: string, session_id: string, use_cache?: boolean, use_hash_lookup?: boolean) {
    super(Kind.VerdictRequestForUrl);
    this.url = url.toString();
    this.session_id = session_id;
    this.guid = guid;
    this.use_cache = use_cache;
    this.use_hash_lookup = use_hash_lookup;
  }

  @JsonProperty() public url: string;
  @JsonProperty() public guid: string;
  @JsonProperty() public session_id: string;
  @JsonProperty() public use_hash_lookup?: boolean;
  @JsonProperty() public use_cache?: boolean;  
}
