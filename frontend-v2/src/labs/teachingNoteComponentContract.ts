import TeachingNoteBadge from '@/components/business/TeachingNoteBadge.vue'
import TeachingNoteBindingSummary from '@/components/business/TeachingNoteBindingSummary.vue'
import TeachingNoteBindingTargetView from '@/components/business/TeachingNoteBindingTargetView.vue'
import TeachingNoteCard from '@/components/business/TeachingNoteCard.vue'
import TeachingNoteDeleteConfirm from '@/components/business/TeachingNoteDeleteConfirm.vue'
import TeachingNoteEditor from '@/components/business/TeachingNoteEditor.vue'
import TeachingNoteList from '@/components/business/TeachingNoteList.vue'
import type {
  NoteType,
  TeachingNoteEffectLevel,
  TeachingNoteEditorValue,
  TeachingNoteListState,
  TeachingNoteModel,
  TeachingNoteTargetType,
} from '@/types'

export const teachingNoteTargetTypes: TeachingNoteTargetType[] = [
  'ContentBlock',
  'Section',
  'AtomicSection',
  'AtomicSectionPanel',
  'AtomicSectionItem',
  'SectionItem',
]

export const teachingNoteTypes: NoteType[] = [
  'General',
  'ClassroomRecord',
  'LearningEffect',
  'TeachingReflection',
  'RevisionSuggestion',
  'QuestionReplacement',
  'CommonMistake',
]

export const teachingNoteEffectOptions: Array<TeachingNoteEffectLevel | null> = [
  null,
  'Good',
  'Normal',
  'Weak',
  'Failed',
]

export const teachingNoteListStates: TeachingNoteListState[] = [
  'idle',
  'loading',
  'empty',
  'error',
]

export const teachingNoteDefaultEditorValue: TeachingNoteEditorValue = {
  noteType: 'General',
  content: '',
  effectLevel: null,
  occurredAt: null,
  bindings: [
    {
      targetType: 'ContentBlock',
      targetId: 3001,
    },
  ],
}

export const teachingNoteComponentContract = {
  components: {
    TeachingNoteBadge,
    TeachingNoteBindingSummary,
    TeachingNoteBindingTargetView,
    TeachingNoteCard,
    TeachingNoteDeleteConfirm,
    TeachingNoteEditor,
    TeachingNoteList,
  },
  sampleNote: {
    id: 1,
    noteType: 'RevisionSuggestion',
    content: 'Add one simpler transition example before using the circular-track threshold problem.',
    effectLevel: null,
    occurredAt: null,
    bindings: teachingNoteDefaultEditorValue.bindings,
    createdTime: '2026-06-24T10:00:00+08:00',
    updatedTime: '2026-06-24T10:30:00+08:00',
  } satisfies TeachingNoteModel,
} as const
