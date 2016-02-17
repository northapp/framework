﻿import * as React from "react"
import { Router, Route, Redirect, IndexRoute } from "react-router"
import { Button, OverlayTrigger, Tooltip, MenuItem, DropdownButton } from "react-bootstrap"
import { Dic, hasFlag } from './Globals';
import { ajaxGet, ajaxPost } from './Services';
import { openModal } from './Modals';
import { IEntity, Lite, Entity, ModifiableEntity, EmbeddedEntity, LiteMessage, OperationMessage, EntityPack,
    OperationSymbol, ConstructSymbol_From, ConstructSymbol_FromMany, ConstructSymbol_Simple, ExecuteSymbol, DeleteSymbol, Basics } from './Signum.Entities';
import { PropertyRoute, PseudoType, EntityKind, TypeInfo, IType, Type, getTypeInfo, OperationInfo, OperationType, GraphExplorer  } from './Reflection';
import { TypeContext } from './TypeContext';
import * as Finder from './Finder';
import * as Navigator from './Navigator';
import * as QuickLinks from './QuickLinks';
import * as ContexualItems from './SearchControl/ContextualItems';
import ButtonBar from './Frames/ButtonBar';
import { EntityFrame }  from './Lines';
import { getEntityOperationButtons }  from './Operations/EntityOperations';
import { getConstructFromManyContextualItems, getEntityOperationsContextualItems }  from './Operations/ContextualOperations';
import { ContextualItemsContext }  from './SearchControl/ContextualItems';

export function start() {
    ButtonBar.onButtonBarRender.push(getEntityOperationButtons);
    ContexualItems.onContextualItems.push(getConstructFromManyContextualItems);
    ContexualItems.onContextualItems.push(getEntityOperationsContextualItems);
    QuickLinks.registerGlobalQuickLink(ctx => new QuickLinks.QuickLinkExplore({
        queryName: Basics.OperationLogEntity_Type,
        simpleColumnName: "Target",
        simpleValue: ctx.lite
    }, { isVisible: getTypeInfo(ctx.lite.EntityType).requiresSaveOperation }));
}

export const operationSettings: { [operationKey: string]: OperationSettings } = {};

export function addSettings(...settings: OperationSettings[]) {
    settings.forEach(s => Dic.addOrThrow(operationSettings, s.operationSymbol.key, s));
}


export function getSettings(operation: OperationSymbol | string): OperationSettings {
    const operationKey = (operation as OperationSymbol).key || operation as string; 

    return operationSettings[operationKey];
}

var isOperationAllowed = (oi: OperationInfo) => true;

export function operationInfos(ti: TypeInfo) {
    return Dic.getValues(ti.operations).filter(isOperationAllowed);
}

/**
 * Operation Settings
 */
export abstract class OperationSettings {

    text: () => string;
    operationSymbol: OperationSymbol;

    constructor(operationSymbol: OperationSymbol) {
        this.operationSymbol = operationSymbol;
    }
}



/**
 * Constructor Operation Settings
 */
export class ConstructorOperationSettings<T extends Entity> extends OperationSettings {

    isVisible: (ctx: ConstructorOperationContext<T>) => boolean;
    onConstruct: (ctx: ConstructorOperationContext<T>) => Promise<T>;

    constructor(operationSymbol: ConstructSymbol_Simple<T>, options: ConstructorOperationOptions<T>) {
        super(operationSymbol);

        Dic.extend(this, options);
    }
}
 
export interface ConstructorOperationOptions<T extends Entity> {
    text?: () => string;
    isVisible?: (ctx: ConstructorOperationContext<T>) => boolean;
    onConstruct?: (ctx: ConstructorOperationContext<T>) => Promise<T>;
}

export interface ConstructorOperationContext<T extends Entity> {
    operationInfo: OperationInfo;
    settings: ConstructorOperationSettings<T>
}



/**
 * Contextual Operation Settings
 */
export class ContextualOperationSettings<T extends Entity> extends OperationSettings {

    isVisible: (ctx: ContextualOperationContext<T>) => boolean;
    hideOnCanExecute: boolean;
    confirmMessage: (ctx: ContextualOperationContext<T>) => string;
    onClick: (ctx: ContextualOperationContext<T>, event: React.MouseEvent) => void;
    style: string;
    order: number;

    constructor(operationSymbol: ExecuteSymbol<T> | DeleteSymbol<T> | ConstructSymbol_From<any, T> | ConstructSymbol_FromMany<any, T>, options: ContextualOperationOptions<T>) {
        super(operationSymbol);

        Dic.extend(this, options);
    }
}

export interface ContextualOperationOptions<T extends Entity> {
    text: () => string;
    isVisible: (ctx: ContextualOperationContext<T>) => boolean;
    hideOnCanExecute: boolean;
    confirmMessage: (ctx: ContextualOperationContext<T>) => string;
    onClick: (ctx: ContextualOperationContext<T>, event: React.MouseEvent) => void;
    style: string;
    order: number;
}

export interface ContextualOperationContext<T extends Entity> {
    context: ContextualItemsContext
    operationInfo: OperationInfo;
    settings: ContextualOperationSettings<T>;
    entityOperationSettings: EntityOperationSettings<T>;
    canExecute: string;
}



export interface EntityOperationContext<T extends Entity> {
    frame: EntityFrame<T>;
    entity: T;
    operationInfo: OperationInfo;
    settings: EntityOperationSettings<T>;
    canExecute: string;
}

export class EntityOperationSettings<T extends Entity> extends OperationSettings {

    contextual: ContextualOperationSettings<T>;
    contextualFromMany: ContextualOperationSettings<T>;
    
    isVisible: (ctx: EntityOperationContext<T>) => boolean;
    confirmMessage: (ctx: EntityOperationContext<T>) => string;
    onClick: (ctx: EntityOperationContext<T>) => void;
    hideOnCanExecute: boolean;
    group: EntityOperationGroup;
    order: number;
    style: string;

    constructor(operationSymbol: ExecuteSymbol<T> | DeleteSymbol<T> | ConstructSymbol_From<any, T>, options: EntityOperationOptions<T>) {
        super(operationSymbol)

        Dic.extend(this, options);

        this.contextual = options.contextual ? new ContextualOperationSettings(operationSymbol, options.contextual) : null;
        this.contextualFromMany = options.contextualFromMany ? new ContextualOperationSettings(operationSymbol, options.contextualFromMany) : null;
    }
}

export interface EntityOperationOptions<T extends Entity> {
    contextual?: ContextualOperationOptions<T>;
    contextualFromMany?: ContextualOperationOptions<T>;

    text?: () => string;
    isVisible?: (ctx: EntityOperationContext<T>) => boolean;
    confirmMessage?: (ctx: EntityOperationContext<T>) => string;
    onClick?: (ctx: EntityOperationContext<T>) => void;
    hideOnCanExecute?: boolean;
    group?: EntityOperationGroup;
    order?: number;
    style?: string;
}



export var CreateGroup: EntityOperationGroup = {
    key: "create",
    text: () => OperationMessage.Create.niceToString(),
    simplifyName: cs => {
        var array = new RegExp(OperationMessage.CreateFromRegex.niceToString()).exec(cs);
        return array ? array[1].firstUpper() : cs;
    },
    cssClass: "sf-operation",
    order: 200,
};

export interface EntityOperationGroup {
    key: string;
    text: () => string;
    simplifyName?: (complexName: string) => string;
    cssClass?: string;
    order?: number;
}

export function autoStyleFunction(oi: OperationInfo) {
    return oi.operationType == OperationType.Delete ? "danger" :
        oi.operationType == OperationType.Execute && oi.key.endsWith(".Save") ? "primary" : "default";
}


export function isEntityOperation(operationType: OperationType) {
    return operationType == OperationType.ConstructorFrom ||
        operationType == OperationType.Execute ||
        operationType == OperationType.Delete;
}

export namespace API {

    export function construct<T extends Entity>(operationKey: string | ConstructSymbol_Simple<T>, args?: any[]): Promise<EntityPack<T>> {
        return ajaxPost<EntityPack<T>>({ url: "/api/operation/construct" }, { operationKey: getKey(operationKey), args: args });
    }

    export function constructFromEntity<T extends Entity, F extends Entity>(entity: F, operationKey: string | ConstructSymbol_From<T, F>, args?: any[]): Promise<EntityPack<T>> {
        new GraphExplorer().propagateModified(entity, args);
        return ajaxPost<EntityPack<T>>({ url: "/api/operation/constructFromEntity" }, { entity: entity, operationKey: getKey(operationKey), args: args } as EntityOperationRequest);
    }

    export function constructFromLite<T extends Entity, F extends Entity>(lite: Lite<F>, operationKey: string | ConstructSymbol_From<T, F>, args?: any[]): Promise<EntityPack<T>> {
        new GraphExplorer().propagateModified(lite, args);
        return ajaxPost<EntityPack<T>>({ url: "/api/operation/constructFromLite" }, { lite: lite, operationKey: getKey(operationKey), args: args } as LiteOperationRequest);
    }

    export function constructFromMultiple<T extends Entity, F extends Entity>(lites: Lite<F>[], operationKey: string | ConstructSymbol_From<T, F>, args?: any[]): Promise<ErrorReport> {
        new GraphExplorer().propagateModified(lites, args);
        return ajaxPost<ErrorReport>({ url: "/api/operation/constructFromMultiple" }, { lites: lites, operationKey: getKey(operationKey), args: args } as MultiOperationRequest);
    }

    export function constructFromMany<T extends Entity, F extends Entity>(lites: Lite<F>[], operationKey: string | ConstructSymbol_From<T, F>, args?: any[]): Promise<EntityPack<T>> {
        new GraphExplorer().propagateModified(lites, args);
        return ajaxPost<EntityPack<T>>({ url: "/api/operation/constructMany" }, { lites: lites, operationKey: getKey(operationKey), args: args } as MultiOperationRequest);
    }

    export function executeEntity<T extends Entity>(entity: T, operationKey: string | ExecuteSymbol<T>, args?: any[]): Promise<EntityPack<T>> {
        new GraphExplorer().propagateModified(entity, args);
        return ajaxPost<EntityPack<T>>({ url: "/api/operation/executeEntity" }, { entity: entity, operationKey: getKey(operationKey), args: args } as EntityOperationRequest);
    }

    export function executeLite<T extends Entity>(lite: Lite<T>, operationKey: string | ExecuteSymbol<T>, args?: any[]): Promise<EntityPack<T>> {
        new GraphExplorer().propagateModified(lite, args);
        return ajaxPost<EntityPack<T>>({ url: "/api/operation/executeLite" }, { lite: lite, operationKey: getKey(operationKey), args: args } as LiteOperationRequest);
    }

    export function executeMultiple<T extends Entity>(lites: Lite<T>[], operationKey: string | ExecuteSymbol<T>, args?: any[]): Promise<ErrorReport> {
        new GraphExplorer().propagateModified(lites, args);
        return ajaxPost<ErrorReport>({ url: "/api/operation/executeMultiple" }, { lites: lites, operationKey: getKey(operationKey), args: args } as MultiOperationRequest);
    }

    export function deleteEntity<T extends Entity>(entity: T, operationKey: string | ExecuteSymbol<T>, args?: any[]): Promise<void> {
        new GraphExplorer().propagateModified(entity, args);
        return ajaxPost<void>({ url: "/api/operation/deleteEntity" }, { entity: entity, operationKey: getKey(operationKey), args: args } as EntityOperationRequest);
    }

    export function deleteLite<T extends Entity>(lite: Lite<T>, operationKey: string | ExecuteSymbol<T>, args?: any[]): Promise<void> {
        new GraphExplorer().propagateModified(lite, args);
        return ajaxPost<void>({ url: "/api/operation/deleteLite" }, { lite: lite, operationKey: getKey(operationKey), args: args } as LiteOperationRequest);
    }

    export function deleteMultiple<T extends Entity>(lites: Lite<T>[], operationKey: string | ExecuteSymbol<T>, args?: any[]): Promise<ErrorReport> {
        new GraphExplorer().propagateModified(lites, args);
        return ajaxPost<ErrorReport>({ url: "/api/operation/deleteMultiple" }, { lites: lites, operationKey: getKey(operationKey), args: args } as MultiOperationRequest);
    }

    export interface ErrorReport {
        errors: { [liteKey: string]: string; }
    }


  

    function getKey(operationKey: string | OperationSymbol) {
        return (operationKey as OperationSymbol).key || operationKey as string;
    }



    interface MultiOperationRequest {
        operationKey: string;
        type?: string;
        lites: Lite<Entity>[];
        args: any[]
    }

    interface ConstructOperationRequest {
        operationKey: string;
        type?: string;
        args: any[];
    }


    interface EntityOperationRequest {
        operationKey: string;
        entity: Entity;
        type?: string;
        args: any[];
    }

    interface LiteOperationRequest {
        operationKey: string;
        lite: Lite<Entity>;
        type?: string;
        args: any[];
    }


    export function stateCanExecutes<T extends Entity>(lites: Lite<T>[], operationKeys: string[]): Promise<CanExecutesResponse> {
        return ajaxPost<CanExecutesResponse>({ url: "/api/operation/stateCanExecutes" }, { lites, operationKeys });
    }

    export interface CanExecutesResponse {
        canExecutes: { [operationKey: string]: string };
    }
}

