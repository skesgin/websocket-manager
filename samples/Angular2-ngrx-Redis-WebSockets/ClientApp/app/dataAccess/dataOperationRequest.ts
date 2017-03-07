export class DataOperationRequest {
    public operationId: string;
    public controller: string;
    public action: string;
    public arguments: any[];

    constructor() {
        this.arguments = new Array<any>();
    }
}
