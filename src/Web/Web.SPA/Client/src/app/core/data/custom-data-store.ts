import CustomStore from "devextreme/data/custom_store";

export abstract class CustomDataStore<TGET = any, TKEY = number> extends CustomStore<TGET, TKEY>{
    public subjectId: TKEY = null;
}