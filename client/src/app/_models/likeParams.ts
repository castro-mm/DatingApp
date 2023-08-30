import { BaseParams } from "./baseParams";

export class LikeParams extends BaseParams {
    predicate: string = 'liked';

    constructor() {
        super();
    }
}

