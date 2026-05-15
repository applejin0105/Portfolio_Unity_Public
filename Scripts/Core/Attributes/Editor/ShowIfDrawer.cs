using System;
using System.Globalization;
using Core.Attributes.Drawer;
using UnityEditor;
using UnityEngine;

namespace Core.Attributes.Editor
{
    // 앞선 ShowIfAttribute에서 받은 내용을 바탕으로 이제 작접 '어떻게 그려질지' 제어

    // CustomPropertyDrawer: 기본 인스펙터 UI를 덮어 씌우는 커스텀 프로퍼티임을 선언. 유니티가 이를 통해 에디터 UI 렌더링 과정에 끼워줌.
    // typeof는 메모리상에 존재하는 해당 클래스 설계도를 넘겨준다. 따라서 CustomPropertyDrawer의 매개변수로 ShowIfAttribute를 전달해준다!
    // 굳이 이렇게 한 이유는 View와 Data를 구분하는 고-수의 방법을 쓰기 위함이다. 모델-뷰-뷰모델. 캬.

    // 따라서 ShowIfAttribute는 데이터(Model)로, 어떤 조건일 때 보여줄 것인가 라는 정보만 가진다. 화면에 어떻게 그려질지에 대해서는 전혀 모른다.
    // 반면 ShoIfDrawer는 화면(View)으로, 인스펙터에 여백을 얼마나 주고, 필드를 어떻게 그릴지 렌더링 로직만 가진다.
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        private ShowIfAttribute ShowIf => (ShowIfAttribute)attribute;

        /* 인스펙터에서 이 변수가 차지할 높이를 계산한다.
         * IsConditionMet이 true이면 기본 변수 높이를 반환, false이면 -EditorGUIUtility.standardVerticalSpacing(음수 여백)을 반환
         * 유니티 인스펙터에서 변수를 완전히 숨기려면 단순히 그리지 않는 것이 아니라(OnGUI 비우기) 높이를 0이나 음수 마진으로 만들어줘야지만 빈 공간이 남지 않는다.
         * 그러면 그냥 0.0f 쓰는게 뭔가 더 직관적(?)으로 좋지 않나, 싶어서 찾아보니 다음과 같다고 하더라...
         * 유니티 인스펙터는 여러 변수들을 아래로 차곡차곡 쌓아서 그릴 때, 단순히 '프로퍼티의 높이'만 계산해서 이어 붙이지 않고 가독성을 위해 각 변수마다 기본적인 수직여백(Standard Vertical Spacing, 보통 2픽셀)을 자동으로 끼워 넣는다.
         * 그러다보니 return 0.0f;을 하면, 2픽셀 여백은 그대로 남아버린다.
         * 유니티가 강제로 더하려는 이 기본 여백을 미리 음수 값으로 상쇄(Cancel out)하는 고오오오급 테크닉이다.
         * 이게 깔끔한 커스텀 에디터를 만들기 위한 정석적인 패턴이라 함.
         * 실제로 해보니 2픽셀이 좀 보기 싫다.
         */
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsConditionMet(property))
                return EditorGUI.GetPropertyHeight(property, label, true);

            return -EditorGUIUtility.standardVerticalSpacing;
        }

        /* 실제로 인스펙터에 변수를 그리는 역할
         * IsConditionMat 조건이 참일 때만 EditorGUI.PropertyField를 호출하여 화면에 렌더링
         */
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (IsConditionMet(property)) EditorGUI.PropertyField(position, property, label, true);
        }

        /* 저장된 조건식 문자열(예: A==1 || B==2 && C==3)을 분석하여 최종적으로 참/거짓을 반환.
         * 1. 띄어쓰기 모두 제거, 파싱을 단순화
         * 2. ||를 기준으로 문자열을 쪼개서 그룹 생성
         * 3. 각 OR 그룹 내에서 다시 && 기준으로 세부 조건 쪼갬
         * 4. 모든 AND 조건들을 통과하면 해당 OR 그룹이 참이 되고, 전체 결과도 참으로 반환
         *
         *
         * 여기서 재미있게 생각해볼만한게, 조건식에 대해 조금만 상식이 있어도 이거 조금만 복잡해져도, 가령 괄호만 추가해도 100프로 꼬인다.
         * [ShowIf("A && (B || C)")] 이런게 있다고 하면 ||가 우선순위이고, "A&&(B" "C)" 이렇게 나뉘는데 그럼 변수를 EvaluateSingleCondition에서 "A"와 "(B", 그리고 "C)"를 찾는다.
         * 그럼 당연히 없으니깐 항상 false 출력하거나 뭐 예외 방지용으로 true가 출력 될 수 있다.
         * 그런데 괄호가 없다면, ||를 먼저 자르는건 일종의 트릭이다.
         * C#이랑 수학 연산에서는 AND가 OR보다 우선순위가 더 높다. 가령 A || B && C라는 식은 C#에서 A || (B && C)로 해석된다.
         * 그래서 아래처럼 간단하게 구현해도, || 우선으로 하면 어느정도 식이 커버된다.
         *
         * 그럼 복잡해지면 ㅈ되는거이닌가? 싶지만, 상식적으로, 이거 설계할때도 감안한거긴 한데, ShowIf 내부 조건 식이 복잡해지는건 프로그래머의 설계상 미스다.
         * 단순 에디터 코드를 짜는건데, 인스펙터에 이 변수 띄울지 말지 결정하는건데 이걸 드럽게 복잡하게 구성해야 하면, 그건 변수 구조가 잘못된거다.
         *
         * 핵심은 단순하게! 에디터에서 너무 복잡하지 않게 사용하는것뿐이다.
         */
        private bool IsConditionMet(SerializedProperty property)
        {
            var expr = ShowIf.ConditionExpression;

            // 식이 비어있거나 없으면 당연히 그냥 보여야함.
            if (string.IsNullOrEmpty(expr)) return true;

            // 띄어쓰기 제거 (파싱을 쉽게 하기 위함)
            expr = expr.Replace(" ", "");

            // || (OR) 기준으로 먼저 그룹을 나눔
            // .Split(...): 문자열을 특정 기준(구분자)으로 쪼개어 문자열 배열(string[])로 반환하는 C#의 내장 메서드
            // new[]:  Split 메서드가 배열 형태의 입력값을 요구하기 때문에 사용. 사실 new string[]으로 써야하는데 컴파일러가 이정도는 알아서 해줌
            // { "||" }: 무엇을 기준으로 자를 것인지 지정하는 부분. 여기선 ||를 기준으로 자름
            // StringSplitOptions.RemoveEmptyEntries: 문자열을 잘랐을 때 생길 수 있는 빈 문자열을 결과에서 제외하라는 명령
            var orGroups = expr.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var orGroup in orGroups)
            {
                var andResult = true;

                // && (AND) 기준으로 세부 조건 분리
                var andConditions = orGroup.Split(new[] { "&&" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var condition in andConditions)
                    // 하나라도 false면 이 AND 그룹은 실패
                    if (!EvaluateSingleCondition(property, condition))
                    {
                        andResult = false;
                        break;
                    }

                // OR 그룹 중 하나라도 전부 통과(true)했다면 패스
                if (andResult) return true;
            }

            return false;
        }

        /*  단일 조건식 평가 (예: "pulseType==DoTween", "isLoop", "!isFade")
         *  가장 단순한 단일 조건을 평가한다.
         *  비교 연산자가 있는 경우, 변수명과 비교값을 분리해서 FindPropertyRelative로 실제 변수 찾고, CompareStringValue로 값을 비교
         *  비교 연산자가 없는 경우, 즉 bool 단독의 경우, 문자열 맨 앞에 !가 있는지 없는지 확인해서 참/거짓 반환 결정 후, boolValue를 직접 읽어옴.
         */
        private bool EvaluateSingleCondition(SerializedProperty property, string condition)
        {
            var isNotEquals = condition.Contains("!=");
            var isEquals = condition.Contains("==");

            // 비교 연산자가 있는 경우 (==, !=)
            if (isEquals || isNotEquals)
            {
                var separator = isNotEquals ? "!=" : "==";
                var parts = condition.Split(new[] { separator }, StringSplitOptions.None);
                var varName = parts[0];
                var targetValueStr = parts[1];

                var targetProp = FindPropertyRelative(property, varName);
                if (targetProp == null) return true; // 못 찾으면 에러 방지용 true

                var isMatch = CompareStringValue(targetProp, targetValueStr);
                return isNotEquals ? !isMatch : isMatch;
            }
            // 비교 연산자가 없는 경우 (bool 변수 단독 사용)
            else
            {
                var isNot = condition.StartsWith("!"); // 앞에 !가 붙었는지 확인
                var varName = isNot ? condition.Substring(1) : condition;

                var targetProp = FindPropertyRelative(property, varName);
                if (targetProp == null) return true;

                var val = targetProp.boolValue;
                return isNot ? !val : val;
            }
        }

        /* 경로를 찾아 SerializedProperty를 반환하는 헬퍼 함수
         * 현재 그리고 있는 변수와 같은 계층에 있는 다른 변수를 이름으로 찾아 반환
         * 클래스 내부의 중첩된 구조 (배열이나 구조체 내부)에서도 올바른 경로를 찾기 위해 propertyPath.LastIndexOf('.')를 사용하여 부모 경로를 역추적
         */
        private SerializedProperty FindPropertyRelative(SerializedProperty property, string fieldName)
        {
            // 현재 변수의 전체 경로를 문자열로 가져옴
            var propertyPath = property.propertyPath;
            // 경로 문자열에서 가장 마지막에 마침표의 위치를 찾음. 부모 객체와 자식 변수를 구분하는 기호
            var dotIndex = propertyPath.LastIndexOf('.');
            // dotIndex < 0: 마침표가 없다면(최상위 계층이라면) 찾으려는 fieldName이 바로 경로임
            // 없다면 마침표 이전까지의 문자열(부모 경로)를 잘래나고 찾고자 하는 fieldName을 이어 붙임
            var conditionPath = dotIndex < 0 ? fieldName : propertyPath.Substring(0, dotIndex) + "." + fieldName;

            // 가령 현재 경로가 Player.stats.health이고, fieldName이 mana라면
            // 부모 경로인 Player.stats을 추출하고(같은 계층을 탐색하는 과정임!) .mana를 붙여서 Player.stats.mana라는 정확한 목표 경로를 완성한다.
            
            // 완성된 절대 경로를 이용해 실제 대상 변수(SerializedProperty)를 가져와 반환
            var foundProp = property.serializedObject.FindProperty(conditionPath);
            if (foundProp == null)
                Debug.LogWarning($"[ShowIf] '{conditionPath}' 변수를 찾을 수 없습니다!");

            return foundProp;
        }

        /* 찾아낸 목표 변수(prop)의 실제 값과 조건식에 적힌 문자열 값(stringValue)이 일치하는지 비교
         * propertyType(Boolean, Enum, Integer, Float, String)에 따라 switch 문으로 분기, 유니티 직렬화 객체의 값을 문자열 형태로 통일하고,
         * 대소문자 구분 없이(또는 정확히) 비교한다.
         */
        private bool CompareStringValue(SerializedProperty prop, string stringValue)
        {
            return prop.propertyType switch
            {
                // True나 False를 문자열로 바꾼 뒤 .ToLower()를 통해 소문자로 통일. ture든 Ture든 상관없게
                SerializedPropertyType.Boolean => prop.boolValue.ToString().ToLower() == stringValue.ToLower(),
                // Enum은 현재 선택된 이름(string)과 직접 비교
                SerializedPropertyType.Enum => prop.enumNames[prop.enumValueIndex] == stringValue,
                SerializedPropertyType.Integer => prop.intValue.ToString() == stringValue,
                SerializedPropertyType.Float => prop.floatValue.ToString(CultureInfo.InvariantCulture) == stringValue,
                SerializedPropertyType.String => prop.stringValue == stringValue,
                _ => false
            };
        }
    }
}
