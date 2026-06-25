import {
  cmsV2Api,
  type CmsV2CreateTeachingNoteRequest,
  type CmsV2TeachingNoteBindingRequest,
  type CmsV2TeachingNoteDto,
  type CmsV2SearchTeachingNotesQuery,
  type CmsV2TeachingNoteTargetType,
  type CmsV2UpdateTeachingNoteRequest,
} from '@/apis/cmsV2Client'
import type {
  CreateTeachingNoteRequestModel,
  TeachingNoteSearchQuery,
  TeachingNoteBindingModel,
  TeachingNoteModel,
  UpdateTeachingNoteRequestModel,
} from '@/types'

export interface TeachingNoteActionTarget {
  targetType: CmsV2TeachingNoteTargetType
  targetId: number
}

function toBindingRequests(bindings: TeachingNoteBindingModel[]): CmsV2TeachingNoteBindingRequest[] {
  return bindings.map((binding) => ({
    targetType: binding.targetType,
    targetId: binding.targetId,
  }))
}

function toTeachingNoteModel(note: CmsV2TeachingNoteDto): TeachingNoteModel {
  return note
}

export function useTeachingNoteActions() {
  async function loadTargetTeachingNotes(target: TeachingNoteActionTarget) {
    const notes = await cmsV2Api.listTeachingNotesByBinding(target.targetType, target.targetId)
    return notes.map(toTeachingNoteModel)
  }

  async function searchTeachingNotes(query: TeachingNoteSearchQuery) {
    const apiQuery: CmsV2SearchTeachingNotesQuery = {
      keyword: query.keyword,
      noteType: query.noteType,
      effectLevel: query.effectLevel,
      targetType: query.targetType,
      targetId: query.targetId,
      occurredFrom: query.occurredFrom,
      occurredTo: query.occurredTo,
    }
    const notes = await cmsV2Api.listTeachingNotes(apiQuery)
    return notes.map(toTeachingNoteModel)
  }

  async function createTeachingNote(request: CreateTeachingNoteRequestModel) {
    const apiRequest: CmsV2CreateTeachingNoteRequest = {
      noteType: request.noteType,
      content: request.content,
      effectLevel: request.effectLevel ?? null,
      occurredAt: request.occurredAt ?? null,
      bindings: toBindingRequests(request.bindings),
    }

    return toTeachingNoteModel(await cmsV2Api.createTeachingNote(apiRequest))
  }

  async function updateTeachingNote(noteId: number, request: UpdateTeachingNoteRequestModel) {
    const apiRequest: CmsV2UpdateTeachingNoteRequest = {
      noteType: request.noteType,
      content: request.content,
      effectLevel: request.effectLevel === undefined ? undefined : request.effectLevel,
      occurredAt: request.occurredAt === undefined ? undefined : request.occurredAt,
      bindings: request.bindings ? toBindingRequests(request.bindings) : undefined,
    }

    return toTeachingNoteModel(await cmsV2Api.updateTeachingNote(noteId, apiRequest))
  }

  async function deleteTeachingNote(noteId: number) {
    await cmsV2Api.deleteTeachingNote(noteId)
  }

  return {
    loadTargetTeachingNotes,
    searchTeachingNotes,
    createTeachingNote,
    updateTeachingNote,
    deleteTeachingNote,
  }
}
