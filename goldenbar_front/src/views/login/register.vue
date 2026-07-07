<template>
  <div class="register-container">
    <div class="register-box">
      <h2 class="register-title">회원가입</h2>
      <div class="required-notice">
        <span class="required-mark">✔</span> 표시는 반드시 입력하셔야 하는 항목입니다.
      </div>

      <el-form ref="registerFormRef" :model="form" :rules="rules" label-width="0">
        <table class="register-table">
          <tbody>
            <!-- 아이디 -->
            <tr>
              <th class="required">아이디</th>
              <td>
                <div class="flex-row">
                  <el-form-item prop="username" class="mb-0">
                    <el-input v-model="form.username" placeholder="아이디는 영문 또는 숫자로 입력해 주세요" />
                  </el-form-item>
                  <el-button @click="checkDuplicateId" class="ml-2">중복확인</el-button>
                </div>
              </td>
            </tr>

            <!-- 비밀번호 -->
            <tr>
              <th class="required">비밀번호</th>
              <td>
                <el-form-item prop="password" class="mb-0">
                  <el-input v-model="form.password" type="password" placeholder="10자 이상 영문대/소문자, 숫자, 특수문자 조합" show-password />
                </el-form-item>
              </td>
            </tr>

            <!-- 이름 -->
            <tr>
              <th class="required">이름</th>
              <td>
                <el-form-item prop="name" class="mb-0">
                  <el-input v-model="form.name" />
                </el-form-item>
              </td>
            </tr>

            <!-- 주민등록번호/법인번호 -->
            <tr>
              <th>주민등록번호/법인번호</th>
              <td>
                <div class="flex-row ssn-row">
                  <el-input v-model="form.ssn1" maxlength="6" />
                  <span class="separator">-</span>
                  <el-input v-model="form.ssn2" type="password" maxlength="7" show-password />
                </div>
              </td>
            </tr>

            <!-- 이메일 주소 -->
            <tr>
              <th>이메일 주소</th>
              <td>
                <div class="flex-row email-row">
                  <el-input v-model="form.email1" />
                  <span class="separator">@</span>
                  <el-input v-model="form.email2" />
                </div>
              </td>
            </tr>

            <!-- 휴대폰 번호 -->
            <tr>
              <th class="required">휴대폰 번호</th>
              <td>
                <div class="flex-row phone-row">
                  <el-form-item prop="phone1" class="mb-0">
                    <el-select v-model="form.phone1">
                      <el-option label="010" value="010" />
                      <el-option label="011" value="011" />
                      <el-option label="016" value="016" />
                      <el-option label="017" value="017" />
                      <el-option label="018" value="018" />
                      <el-option label="019" value="019" />
                    </el-select>
                  </el-form-item>
                  <span class="separator">-</span>
                  <el-form-item prop="phone2" class="mb-0">
                    <el-input v-model="form.phone2" maxlength="4" />
                  </el-form-item>
                  <span class="separator">-</span>
                  <el-form-item prop="phone3" class="mb-0">
                    <el-input v-model="form.phone3" maxlength="4" />
                  </el-form-item>
                </div>
              </td>
            </tr>

            <!-- 집 주소 -->
            <tr>
              <th>집 주소</th>
              <td>
                <div class="address-group">
                  <div class="flex-row">
                    <el-input v-model="form.zipCode" readonly placeholder="우편번호" class="zipcode-input" />
                    <el-button @click="searchAddress('home')" class="ml-2">우편번호 찾기</el-button>
                  </div>
                  <el-input v-model="form.addressBase" readonly placeholder="기본주소" class="mt-2" />
                  <el-input v-model="form.addressDetail" placeholder="나머지주소" class="mt-2" />
                </div>
              </td>
            </tr>

            <!-- 상호명 -->
            <tr>
              <th class="required">상호명</th>
              <td>
                <el-form-item prop="companyName" class="mb-0">
                  <el-input v-model="form.companyName" placeholder="예) [부산]황금캐스팅 (물류, 소매는 상호앞에 꼭 지역을 표시하세요)" />
                </el-form-item>
              </td>
            </tr>

            <!-- 사업자 등록번호 -->
            <tr>
              <th>사업자 등록번호</th>
              <td>
                <el-input v-model="form.companyBusinessNumber" />
              </td>
            </tr>

            <!-- 사업자 등록증 파일첨부 -->
            <tr>
              <th>사업자 등록증 파일첨부</th>
              <td>
                <div class="file-upload-row">
                  <input type="file" ref="fileInput" @change="handleFileChange" class="file-input-native" accept="image/*,.pdf" />
                  <span class="file-help">업로드 버튼을 클릭하셔야 해당 파일이 첨부됩니다.</span>
                </div>
              </td>
            </tr>

            <!-- 업태 -->
            <tr>
              <th>업태</th>
              <td>
                <el-input v-model="form.companyBusinessType" />
              </td>
            </tr>

            <!-- 종목 -->
            <tr>
              <th>종목</th>
              <td>
                <el-input v-model="form.companyBusinessCategory" />
              </td>
            </tr>

            <!-- 상호 전화번호 -->
            <tr>
              <th>상호 전화번호</th>
              <td>
                <div class="flex-row phone-row">
                  <el-input v-model="form.companyPhone1" maxlength="4" />
                  <span class="separator">-</span>
                  <el-input v-model="form.companyPhone2" maxlength="4" />
                  <span class="separator">-</span>
                  <el-input v-model="form.companyPhone3" maxlength="4" />
                </div>
              </td>
            </tr>

            <!-- 상호주소 -->
            <tr>
              <th>상호주소</th>
              <td>
                <div class="address-group">
                  <div class="flex-row">
                    <el-input v-model="form.companyZipCode" readonly placeholder="우편번호" class="zipcode-input" />
                    <el-button @click="searchAddress('company')" class="ml-2">우편번호 찾기</el-button>
                  </div>
                  <el-input v-model="form.companyAddressBase" readonly placeholder="기본주소" class="mt-2" />
                  <el-input v-model="form.companyAddressDetail" placeholder="나머지주소" class="mt-2" />
                </div>
              </td>
            </tr>

            <!-- 회원분류 -->
            <tr>
              <th class="required">회원분류</th>
              <td>
                <div class="flex-row">
                  <el-form-item prop="userType" class="mb-0">
                    <el-select v-model="form.userType" placeholder="선택">
                      <el-option label="소매" value="RETAIL" />
                      <el-option label="공장" value="MANUFACTURER" />
                      <el-option label="물류" value="LOGISTICS" />
                    </el-select>
                  </el-form-item>
                  <span class="ml-3 help-text">등록자는 회원분류에 꼭 (공장,소매) 중에서 선택 하시고,(물류는 관리자 문의)</span>
                </div>
              </td>
            </tr>

            <!-- 물류코드 -->
            <tr>
              <th>물류코드</th>
              <td>
                <div class="flex-row">
                  <el-input v-model="form.logisticsCode" />
                  <span class="ml-3 help-text">소매점인 경우 물류센터 코드필요 (물류코드 없는 경우 차후 입력 가능)</span>
                </div>
              </td>
            </tr>

            <!-- 문자 메세지 수신 여부 -->
            <tr>
              <th>문자 메세지 수신 여부</th>
              <td>
                <el-radio-group v-model="form.smsAllowed">
                  <el-radio :label="true">수신 (사절 안함)</el-radio>
                  <el-radio :label="false">사절 (수신 거부)</el-radio>
                </el-radio-group>
              </td>
            </tr>

            <!-- 자기 소개 -->
            <tr>
              <th>자기 소개</th>
              <td>
                <el-input v-model="form.introduction" type="textarea" :rows="4" />
              </td>
            </tr>
          </tbody>
        </table>

        <div class="action-buttons">
          <el-button size="large" @click="$router.push('/login')">로그인</el-button>
          <el-button type="primary" size="large" @click="submitForm" :loading="loading" class="submit-btn">회원가입</el-button>
        </div>
      </el-form>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { register } from '@/api/user'
import type { FormInstance, FormRules } from 'element-plus'

const router = useRouter()
const registerFormRef = ref<FormInstance>()
const fileInput = ref<HTMLInputElement | null>(null)
const loading = ref(false)
const selectedFile = ref<File | null>(null)

const form = reactive({
  username: '',
  password: '',
  name: '',
  ssn1: '',
  ssn2: '',
  email1: '',
  email2: '',
  phone1: '010',
  phone2: '',
  phone3: '',
  zipCode: '',
  addressBase: '',
  addressDetail: '',
  companyName: '',
  companyBusinessNumber: '',
  companyBusinessType: '',
  companyBusinessCategory: '',
  companyPhone1: '',
  companyPhone2: '',
  companyPhone3: '',
  companyZipCode: '',
  companyAddressBase: '',
  companyAddressDetail: '',
  userType: 'RETAIL',
  logisticsCode: '',
  smsAllowed: true,
  introduction: ''
})

const rules = reactive<FormRules>({
  username: [{ required: true, message: '아이디를 입력해주세요', trigger: 'blur' }],
  password: [{ required: true, message: '비밀번호를 입력해주세요', trigger: 'blur' }],
  name: [{ required: true, message: '이름을 입력해주세요', trigger: 'blur' }],
  phone1: [{ required: true, message: '필수', trigger: 'change' }],
  phone2: [{ required: true, message: '필수', trigger: 'blur' }],
  phone3: [{ required: true, message: '필수', trigger: 'blur' }],
  companyName: [{ required: true, message: '상호명을 입력해주세요', trigger: 'blur' }],
  userType: [{ required: true, message: '회원분류를 선택해주세요', trigger: 'change' }]
})

const checkDuplicateId = () => {
  if (!form.username) {
    ElMessage.warning('아이디를 입력해주세요.')
    return
  }
  // TODO: Implement real check
  ElMessage.success('사용 가능한 아이디입니다.')
}

const searchAddress = (type: 'home' | 'company') => {
  // Mock Daum Postcode API behavior for now
  if (type === 'home') {
    form.zipCode = '12345'
    form.addressBase = '서울 강남구 테헤란로 123'
  } else {
    form.companyZipCode = '67890'
    form.companyAddressBase = '부산 해운대구 센텀중앙로 45'
  }
}

const handleFileChange = (e: Event) => {
  const target = e.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    selectedFile.value = target.files[0]
  } else {
    selectedFile.value = null
  }
}

const submitForm = async () => {
  if (!registerFormRef.value) return
  
  await registerFormRef.value.validate(async (valid) => {
    if (valid) {
      try {
        loading.value = true
        const formData = new FormData()
        
        formData.append('Username', form.username)
        formData.append('Password', form.password)
        formData.append('Name', form.name)
        
        const ssn = form.ssn1 && form.ssn2 ? `${form.ssn1}-${form.ssn2}` : ''
        formData.append('Ssn', ssn)
        
        const email = form.email1 && form.email2 ? `${form.email1}@${form.email2}` : ''
        formData.append('Email', email)
        
        const phone = `${form.phone1}-${form.phone2}-${form.phone3}`
        formData.append('Phone', phone)
        
        formData.append('ZipCode', form.zipCode)
        formData.append('AddressBase', form.addressBase)
        formData.append('AddressDetail', form.addressDetail)
        
        formData.append('CompanyName', form.companyName)
        formData.append('CompanyBusinessNumber', form.companyBusinessNumber)
        formData.append('CompanyBusinessType', form.companyBusinessType)
        formData.append('CompanyBusinessCategory', form.companyBusinessCategory)
        
        const compPhone = (form.companyPhone1 || form.companyPhone2 || form.companyPhone3) 
          ? `${form.companyPhone1}-${form.companyPhone2}-${form.companyPhone3}` 
          : ''
        formData.append('CompanyPhone', compPhone)
        
        formData.append('CompanyZipCode', form.companyZipCode)
        formData.append('CompanyAddressBase', form.companyAddressBase)
        formData.append('CompanyAddressDetail', form.companyAddressDetail)
        
        formData.append('UserType', form.userType)
        formData.append('LogisticsCode', form.logisticsCode)
        formData.append('SmsAllowed', form.smsAllowed ? 'true' : 'false')
        formData.append('Introduction', form.introduction)
        
        if (selectedFile.value) {
          formData.append('businessLicenseFile', selectedFile.value)
        }

        const res = await register(formData)
        if (res.code === 20000) {
          ElMessage.success('회원가입이 완료되었습니다.')
          router.push('/login')
        } else {
          ElMessage.error(res.message || '회원가입 실패')
        }
      } catch (error: any) {
        ElMessage.error(error.message || '오류가 발생했습니다.')
      } finally {
        loading.value = false
      }
    } else {
      ElMessage.warning('필수 항목을 모두 입력해주세요.')
    }
  })
}
</script>

<style lang="scss" scoped>
.register-container {
  min-height: 100vh;
  background-color: #f0f2f5;
  display: flex;
  justify-content: center;
  align-items: flex-start;
  padding: 40px 20px;
}

.register-box {
  background: #fff;
  width: 100%;
  max-width: 900px;
  padding: 40px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  border-radius: 8px;
}

.register-title {
  text-align: center;
  font-size: 24px;
  font-weight: bold;
  margin-bottom: 20px;
  color: #333;
}

.required-notice {
  text-align: right;
  font-size: 13px;
  color: #666;
  margin-bottom: 10px;
  
  .required-mark {
    color: #6a2c91;
    font-weight: bold;
  }
}

.register-table {
  width: 100%;
  border-collapse: collapse;
  border-top: 2px solid #ccc;
  
  th, td {
    border-bottom: 1px solid #e0e0e0;
    padding: 12px 16px;
    vertical-align: middle;
  }

  th {
    background-color: #f9f9f9;
    text-align: left;
    width: 200px;
    font-weight: 500;
    color: #333;
    font-size: 14px;
    position: relative;
    
    &.required::before {
      content: '✔';
      color: #6a2c91;
      margin-right: 4px;
      font-weight: bold;
    }
  }

  td {
    background-color: #fff;
  }
}

.flex-row {
  display: flex;
  align-items: center;
}

.ssn-row, .email-row, .phone-row {
  .el-input {
    width: 120px;
  }
  .el-select {
    width: 100px;
  }
}

.separator {
  margin: 0 10px;
  color: #666;
}

.zipcode-input {
  width: 100px;
}

.address-group {
  .el-input {
    max-width: 500px;
  }
}

.help-text {
  font-size: 12px;
  color: #6a2c91;
}

.file-upload-row {
  display: flex;
  align-items: center;
  gap: 15px;
  
  .file-input-native {
    border: 1px solid #dcdfe6;
    padding: 5px;
    border-radius: 4px;
    background: #fdfdfd;
  }
  
  .file-help {
    font-size: 12px;
    color: #888;
  }
}

.action-buttons {
  margin-top: 40px;
  display: flex;
  justify-content: center;
  gap: 20px;
  
  .el-button {
    width: 150px;
  }
  
  .submit-btn {
    background-color: #6a2c91;
    border-color: #6a2c91;
    &:hover {
      background-color: #803fb0;
      border-color: #803fb0;
    }
  }
}
</style>
