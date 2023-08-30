import { BaseParams } from "./baseParams";
import { User } from "./user";

export class UserParams extends BaseParams {

    gender: string = "";
    minAge: number = 18;
    maxAge: number = 99;

    constructor(user: User) {
        super();
        this.gender = user.gender === 'female' ? 'male' : 'female';
    }
}